using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class TechprintUtility
	{
		public static IEnumerable<ResearchProjectDef> GetResearchProjectsNeedingTechprintsNow(Faction faction, List<ThingDef> alreadyGeneratedTechprints = null, float maxMarketValue = float.MaxValue)
		{
			return DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(delegate(ResearchProjectDef p)
			{
				if (p.TechprintCount == 0)
				{
					return false;
				}
				if (p.IsFinished || p.TechprintRequirementMet)
				{
					return false;
				}
				if (faction != null && (p.heldByFactionCategoryTags == null || !p.heldByFactionCategoryTags.Contains(faction.def.categoryTag)))
				{
					return false;
				}
				if (maxMarketValue != float.MaxValue && p.Techprint.BaseMarketValue > maxMarketValue)
				{
					return false;
				}
				if (alreadyGeneratedTechprints != null)
				{
					CompProperties_Techprint compProperties = p.Techprint.GetCompProperties<CompProperties_Techprint>();
					if (compProperties != null)
					{
						int num = compProperties.project.TechprintCount - compProperties.project.TechprintsApplied;
						int num2 = 0;
						for (int i = 0; i < alreadyGeneratedTechprints.Count; i++)
						{
							if (alreadyGeneratedTechprints[i] == p.Techprint)
							{
								num2++;
							}
						}
						if (num2 >= num)
						{
							return false;
						}
					}
				}
				return true;
			});
		}

		public static float GetSelectionWeight(ResearchProjectDef project)
		{
			return project.techprintCommonality * (project.PrerequisitesCompleted ? 1f : 0.02f);
		}

		public static bool TryGetTechprintDefToGenerate(Faction faction, out ThingDef result, List<ThingDef> alreadyGeneratedTechprints = null, float maxMarketValue = float.MaxValue)
		{
			if (!GetResearchProjectsNeedingTechprintsNow(faction, alreadyGeneratedTechprints, maxMarketValue).TryRandomElementByWeight(GetSelectionWeight, out var result2))
			{
				result = null;
				return false;
			}
			result = result2.Techprint;
			return true;
		}

		[DebugOutput]
		public static void TechprintsFromFactions()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Techprints generated from these factions:");
			foreach (Faction item in Find.FactionManager.AllFactions.Where((Faction fa) => fa.def.humanlikeFaction && !fa.Hidden))
			{
				stringBuilder.AppendLine(item.Name);
				for (int i = 0; i < 30; i++)
				{
					if (TryGetTechprintDefToGenerate(item, out var result))
					{
						stringBuilder.AppendLine("    " + result.LabelCap);
						continue;
					}
					stringBuilder.AppendLine("    none possible");
					break;
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		public static void TechprintsFromFactionsChances()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (Faction item in Find.FactionManager.AllFactions.Where((Faction fa) => fa.def.humanlikeFaction && !fa.Hidden))
			{
				Faction localFac = item;
				list.Add(new FloatMenuOption(localFac.Name + " (" + localFac.def.defName + ")", delegate
				{
					List<TableDataGetter<ResearchProjectDef>> list2 = new List<TableDataGetter<ResearchProjectDef>>
					{
						new TableDataGetter<ResearchProjectDef>("defName", (ResearchProjectDef d) => d.defName)
					};
					IEnumerable<ResearchProjectDef> researchProjectsNeedingTechprintsNow = GetResearchProjectsNeedingTechprintsNow(localFac);
					if (researchProjectsNeedingTechprintsNow.Any())
					{
						float sum = researchProjectsNeedingTechprintsNow.Sum((ResearchProjectDef x) => GetSelectionWeight(x));
						list2.Add(new TableDataGetter<ResearchProjectDef>("chance", (ResearchProjectDef x) => (GetSelectionWeight(x) / sum).ToStringPercent()));
						list2.Add(new TableDataGetter<ResearchProjectDef>("weight", (ResearchProjectDef x) => GetSelectionWeight(x).ToString("0.###")));
					}
					DebugTables.MakeTablesDialog(researchProjectsNeedingTechprintsNow, list2.ToArray());
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
