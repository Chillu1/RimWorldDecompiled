using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_NeedAnomalyProject : Alert
{
	private HashSet<KnowledgeCategoryDef> currentCategories = new HashSet<KnowledgeCategoryDef>();

	private List<KnowledgeCategoryDef> missingProjectCategories = new List<KnowledgeCategoryDef>();

	private StringBuilder sb = new StringBuilder();

	public Alert_NeedAnomalyProject()
	{
		defaultLabel = "NeedAnomalyProject".Translate();
		requireAnomaly = true;
	}

	public override AlertReport GetReport()
	{
		if (Find.CurrentMap == null)
		{
			return false;
		}
		if (!Find.Anomaly.AnomalyStudyEnabled)
		{
			return false;
		}
		if (!Find.ResearchManager.AnyProjectIsAvailable)
		{
			return false;
		}
		GetCurrentCategories();
		GetMissingCategories();
		missingProjectCategories.RemoveAll((KnowledgeCategoryDef c) => !Find.ResearchManager.AnyProjectsAvailableWithKnowledgeCategory(c));
		return missingProjectCategories.Count > 0;
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (KnowledgeCategoryDef missingProjectCategory in missingProjectCategories)
		{
			sb.AppendLine(string.Format("  - {0}", "NeedAnomalyProjectDescLine".Translate(missingProjectCategory.label.Named("NAME"))));
		}
		return string.Format("{0}\n{1}\n\n{2}", "NeedAnomalyProjectDesc".Translate(), sb.ToString().TrimEndNewlines(), "NeedAnomalyProjectDescAppended".Translate());
	}

	private void GetMissingCategories()
	{
		missingProjectCategories.Clear();
		foreach (ResearchManager.KnowledgeCategoryProject currentAnomalyKnowledgeProject in Find.ResearchManager.CurrentAnomalyKnowledgeProjects)
		{
			if (currentAnomalyKnowledgeProject.project == null && currentCategories.Contains(currentAnomalyKnowledgeProject.category) && currentAnomalyKnowledgeProject.category.overflowCategory == null)
			{
				missingProjectCategories.Add(currentAnomalyKnowledgeProject.category);
			}
		}
	}

	private void GetCurrentCategories()
	{
		currentCategories.Clear();
		foreach (Thing studiableThingsAndPlatform in Find.StudyManager.GetStudiableThingsAndPlatforms(Find.CurrentMap))
		{
			Thing thing = studiableThingsAndPlatform;
			if (thing is Building_HoldingPlatform building_HoldingPlatform)
			{
				thing = building_HoldingPlatform.HeldPawn;
				CompHoldingPlatformTarget compHoldingPlatformTarget = thing.TryGetComp<CompHoldingPlatformTarget>();
				if (compHoldingPlatformTarget == null || !compHoldingPlatformTarget.CanStudy)
				{
					continue;
				}
			}
			CompStudiable compStudiable = thing.TryGetComp<CompStudiable>();
			if (compStudiable != null && compStudiable.studyEnabled && compStudiable.KnowledgeCategory != null)
			{
				currentCategories.Add(compStudiable.KnowledgeCategory);
			}
		}
	}

	protected override void OnClick()
	{
		Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
		((MainTabWindow_Research)MainButtonDefOf.Research.TabWindow).CurTab = ResearchTabDefOf.Anomaly;
	}
}
