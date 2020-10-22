using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class ResearchProjectDef : Def
	{
		public float baseCost = 100f;

		public List<ResearchProjectDef> prerequisites;

		public List<ResearchProjectDef> hiddenPrerequisites;

		public TechLevel techLevel;

		public List<ResearchProjectDef> requiredByThis;

		private List<ResearchMod> researchMods;

		public ThingDef requiredResearchBuilding;

		public List<ThingDef> requiredResearchFacilities;

		public List<ResearchProjectTagDef> tags;

		public ResearchTabDef tab;

		public float researchViewX = 1f;

		public float researchViewY = 1f;

		[MustTranslate]
		public string discoveredLetterTitle;

		[MustTranslate]
		public string discoveredLetterText;

		[Obsolete]
		public int discoveredLetterMinDifficulty;

		public DifficultyConditionConfig discoveredLetterDisabledWhen = new DifficultyConditionConfig();

		[Obsolete]
		public bool unlockExtremeDifficulty;

		public int techprintCount;

		public float techprintCommonality = 1f;

		public float techprintMarketValue = 1000f;

		public List<string> heldByFactionCategoryTags;

		public DifficultyConditionConfig hideWhen = new DifficultyConditionConfig();

		[Unsaved(false)]
		private float x = 1f;

		[Unsaved(false)]
		private float y = 1f;

		[Unsaved(false)]
		private bool positionModified;

		[Unsaved(false)]
		private ThingDef cachedTechprint;

		[Unsaved(false)]
		private List<Def> cachedUnlockedDefs;

		[Unsaved(false)]
		private List<Dialog_InfoCard.Hyperlink> cachedHyperlinks;

		public const TechLevel MaxEffectiveTechLevel = TechLevel.Industrial;

		private const float ResearchCostFactorPerTechLevelDiff = 0.5f;

		public float ResearchViewX => x;

		public float ResearchViewY => y;

		public float CostApparent => baseCost * CostFactor(Faction.OfPlayer.def.techLevel);

		public float ProgressReal => Find.ResearchManager.GetProgress(this);

		public float ProgressApparent => ProgressReal * CostFactor(Faction.OfPlayer.def.techLevel);

		public float ProgressPercent => Find.ResearchManager.GetProgress(this) / baseCost;

		public bool IsFinished => ProgressReal >= baseCost;

		public bool CanStartNow
		{
			get
			{
				if (!IsFinished && PrerequisitesCompleted && TechprintRequirementMet)
				{
					if (requiredResearchBuilding != null)
					{
						return PlayerHasAnyAppropriateResearchBench;
					}
					return true;
				}
				return false;
			}
		}

		public bool PrerequisitesCompleted
		{
			get
			{
				if (prerequisites != null)
				{
					for (int i = 0; i < prerequisites.Count; i++)
					{
						if (!prerequisites[i].IsFinished)
						{
							return false;
						}
					}
				}
				if (hiddenPrerequisites != null)
				{
					for (int j = 0; j < hiddenPrerequisites.Count; j++)
					{
						if (!hiddenPrerequisites[j].IsFinished)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public int TechprintCount
		{
			get
			{
				if (!ModLister.RoyaltyInstalled)
				{
					return 0;
				}
				return techprintCount;
			}
		}

		public int TechprintsApplied => Find.ResearchManager.GetTechprints(this);

		public bool TechprintRequirementMet
		{
			get
			{
				if (TechprintCount > 0 && Find.ResearchManager.GetTechprints(this) < TechprintCount)
				{
					return false;
				}
				return true;
			}
		}

		public ThingDef Techprint
		{
			get
			{
				if (TechprintCount <= 0)
				{
					return null;
				}
				if (cachedTechprint == null)
				{
					cachedTechprint = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(delegate(ThingDef x)
					{
						CompProperties_Techprint compProperties = x.GetCompProperties<CompProperties_Techprint>();
						return compProperties != null && compProperties.project == this;
					});
					if (cachedTechprint == null)
					{
						Log.ErrorOnce("Could not find techprint for research project " + this, shortHash ^ 0x340C745A);
					}
				}
				return cachedTechprint;
			}
		}

		public List<Def> UnlockedDefs
		{
			get
			{
				if (cachedUnlockedDefs == null)
				{
					cachedUnlockedDefs = (from x in DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.researchPrerequisite == this || (x.researchPrerequisites != null && x.researchPrerequisites.Contains(this))).SelectMany((RecipeDef x) => ((IEnumerable<ThingDefCountClass>)x.products).Select((Func<ThingDefCountClass, Def>)((ThingDefCountClass y) => y.thingDef)))
						orderby x.label
						select x).Concat(from x in DefDatabase<ThingDef>.AllDefs
						where x.researchPrerequisites != null && x.researchPrerequisites.Contains(this)
						orderby x.label
						select x).Concat(from x in DefDatabase<ThingDef>.AllDefs
						where x.plant != null && x.plant.sowResearchPrerequisites != null && x.plant.sowResearchPrerequisites.Contains(this)
						orderby x.label
						select x).Concat(from x in DefDatabase<TerrainDef>.AllDefs
						where x.researchPrerequisites != null && x.researchPrerequisites.Contains(this)
						orderby x.label
						select x)
						.Distinct()
						.ToList();
				}
				return cachedUnlockedDefs;
			}
		}

		public List<Dialog_InfoCard.Hyperlink> InfoCardHyperlinks
		{
			get
			{
				if (cachedHyperlinks == null)
				{
					cachedHyperlinks = new List<Dialog_InfoCard.Hyperlink>();
					List<Def> unlockedDefs = UnlockedDefs;
					if (unlockedDefs != null)
					{
						for (int i = 0; i < unlockedDefs.Count; i++)
						{
							cachedHyperlinks.Add(new Dialog_InfoCard.Hyperlink(unlockedDefs[i]));
						}
					}
				}
				return cachedHyperlinks;
			}
		}

		private bool PlayerHasAnyAppropriateResearchBench
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Building> allBuildingsColonist = maps[i].listerBuildings.allBuildingsColonist;
					for (int j = 0; j < allBuildingsColonist.Count; j++)
					{
						Building_ResearchBench building_ResearchBench = allBuildingsColonist[j] as Building_ResearchBench;
						if (building_ResearchBench != null && CanBeResearchedAt(building_ResearchBench, ignoreResearchBenchPowerStatus: true))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public override void ResolveReferences()
		{
			if (tab == null)
			{
				tab = ResearchTabDefOf.Main;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (techLevel == TechLevel.Undefined)
			{
				yield return "techLevel is Undefined";
			}
			if (ResearchViewX < 0f || ResearchViewY < 0f)
			{
				yield return "researchViewX and/or researchViewY not set";
			}
			if (techprintCount == 0 && !heldByFactionCategoryTags.NullOrEmpty())
			{
				yield return "requires no techprints but has heldByFactionCategoryTags.";
			}
			if (techprintCount > 0 && heldByFactionCategoryTags.NullOrEmpty())
			{
				yield return "requires techprints but has no heldByFactionCategoryTags.";
			}
			List<ResearchProjectDef> rpDefs = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			for (int i = 0; i < rpDefs.Count; i++)
			{
				if (rpDefs[i] != this && rpDefs[i].tab == tab && rpDefs[i].ResearchViewX == ResearchViewX && rpDefs[i].ResearchViewY == ResearchViewY)
				{
					yield return string.Concat("same research view coords and tab as ", rpDefs[i], ": ", ResearchViewX, ", ", ResearchViewY, "(", tab, ")");
				}
			}
			if (!ModLister.RoyaltyInstalled && techprintCount > 0)
			{
				yield return "defines techprintCount, but techprints are a Royalty-specific game system and only work with Royalty installed.";
			}
		}

		public override void PostLoad()
		{
			base.PostLoad();
			if (!ModLister.RoyaltyInstalled)
			{
				techprintCount = 0;
			}
		}

		public float CostFactor(TechLevel researcherTechLevel)
		{
			TechLevel techLevel = (TechLevel)Mathf.Min((int)this.techLevel, 4);
			if ((int)researcherTechLevel >= (int)techLevel)
			{
				return 1f;
			}
			int num = techLevel - researcherTechLevel;
			return 1f + (float)num * 0.5f;
		}

		public bool HasTag(ResearchProjectTagDef tag)
		{
			if (tags == null)
			{
				return false;
			}
			return tags.Contains(tag);
		}

		public bool CanBeResearchedAt(Building_ResearchBench bench, bool ignoreResearchBenchPowerStatus)
		{
			if (requiredResearchBuilding != null && bench.def != requiredResearchBuilding)
			{
				return false;
			}
			if (!ignoreResearchBenchPowerStatus)
			{
				CompPowerTrader comp = bench.GetComp<CompPowerTrader>();
				if (comp != null && !comp.PowerOn)
				{
					return false;
				}
			}
			if (!requiredResearchFacilities.NullOrEmpty())
			{
				CompAffectedByFacilities affectedByFacilities = bench.TryGetComp<CompAffectedByFacilities>();
				if (affectedByFacilities == null)
				{
					return false;
				}
				List<Thing> linkedFacilitiesListForReading = affectedByFacilities.LinkedFacilitiesListForReading;
				int i;
				for (i = 0; i < requiredResearchFacilities.Count; i++)
				{
					if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredResearchFacilities[i] && affectedByFacilities.IsFacilityActive(x)) == null)
					{
						return false;
					}
				}
			}
			return true;
		}

		public void ReapplyAllMods()
		{
			if (researchMods == null)
			{
				return;
			}
			for (int i = 0; i < researchMods.Count; i++)
			{
				try
				{
					researchMods[i].Apply();
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat("Exception applying research mod for project ", this, ": ", ex.ToString()));
				}
			}
		}

		public static ResearchProjectDef Named(string defName)
		{
			return DefDatabase<ResearchProjectDef>.GetNamed(defName);
		}

		public static void GenerateNonOverlappingCoordinates()
		{
			foreach (ResearchProjectDef item in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
			{
				item.x = item.researchViewX;
				item.y = item.researchViewY;
			}
			int num = 0;
			while (true)
			{
				bool flag = false;
				foreach (ResearchProjectDef item2 in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
				{
					foreach (ResearchProjectDef item3 in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
					{
						if (item2 == item3 || item2.tab != item3.tab)
						{
							continue;
						}
						bool num2 = Mathf.Abs(item2.x - item3.x) < 0.5f;
						bool flag2 = Mathf.Abs(item2.y - item3.y) < 0.25f;
						if (num2 && flag2)
						{
							flag = true;
							if (item2.x <= item3.x)
							{
								item2.x -= 0.1f;
								item3.x += 0.1f;
							}
							else
							{
								item2.x += 0.1f;
								item3.x -= 0.1f;
							}
							if (item2.y <= item3.y)
							{
								item2.y -= 0.1f;
								item3.y += 0.1f;
							}
							else
							{
								item2.y += 0.1f;
								item3.y -= 0.1f;
							}
							item2.x += 0.001f;
							item2.y += 0.001f;
							item3.x -= 0.001f;
							item3.y -= 0.001f;
							ClampInCoordinateLimits(item2);
							ClampInCoordinateLimits(item3);
						}
					}
				}
				if (flag)
				{
					num++;
					if (num > 200)
					{
						Log.Error("Couldn't relax research project coordinates apart after " + 200 + " passes.");
						break;
					}
					continue;
				}
				break;
			}
		}

		private static void ClampInCoordinateLimits(ResearchProjectDef rp)
		{
			if (rp.x < 0f)
			{
				rp.x = 0f;
			}
			if (rp.y < 0f)
			{
				rp.y = 0f;
			}
			if (rp.y > 6.5f)
			{
				rp.y = 6.5f;
			}
		}

		public void Debug_ApplyPositionDelta(Vector2 delta)
		{
			bool num = Mathf.Abs(delta.x) > 0.01f;
			bool flag = Mathf.Abs(delta.y) > 0.01f;
			if (num)
			{
				x += delta.x;
			}
			if (flag)
			{
				y += delta.y;
			}
			positionModified = true;
		}

		public void Debug_SnapPositionData()
		{
			x = Mathf.Round(x * 1f) / 1f;
			y = Mathf.Round(y * 20f) / 20f;
			ClampInCoordinateLimits(this);
		}

		public bool Debug_IsPositionModified()
		{
			return positionModified;
		}
	}
}
