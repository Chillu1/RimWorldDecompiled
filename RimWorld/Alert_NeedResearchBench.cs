using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_NeedResearchBench : Alert
{
	private bool HasRequiredResearchBench
	{
		get
		{
			ResearchProjectDef project = Find.ResearchManager.GetProject();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (project.requiredResearchBuilding != null)
				{
					if (maps[i].listerBuildings.ColonistsHaveBuilding(project.requiredResearchBuilding))
					{
						return true;
					}
				}
				else if (maps[i].listerBuildings.ColonistsHaveResearchBench())
				{
					return true;
				}
			}
			return false;
		}
	}

	public Alert_NeedResearchBench()
	{
		defaultLabel = "NeedResearchBench".Translate();
	}

	public override AlertReport GetReport()
	{
		if (Find.CurrentGravship != null)
		{
			return false;
		}
		return Find.ResearchManager.GetProject() != null && !HasRequiredResearchBench;
	}

	protected override void OnClick()
	{
		Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
	}

	public override TaggedString GetExplanation()
	{
		ResearchProjectDef project = Find.ResearchManager.GetProject();
		return "NeedResearchBenchDesc".Translate(project.label, project.requiredResearchBuilding ?? ThingDefOf.SimpleResearchBench) + ("\n\n(" + "ClickToOpenResearchTab".Translate() + ")");
	}
}
