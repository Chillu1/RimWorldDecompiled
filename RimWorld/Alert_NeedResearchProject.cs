using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_NeedResearchProject : Alert
{
	public Alert_NeedResearchProject()
	{
		defaultLabel = "NeedResearchProject".Translate();
		defaultExplanation = "NeedResearchProjectDesc".Translate();
	}

	public override AlertReport GetReport()
	{
		if (Find.AnyPlayerHomeMap == null)
		{
			return false;
		}
		if (Find.ResearchManager.GetProject() != null)
		{
			return false;
		}
		bool flag = false;
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (maps[i].IsPlayerHome && maps[i].listerBuildings.ColonistsHaveResearchBench())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		if (!Find.ResearchManager.AnyProjectIsAvailable)
		{
			return false;
		}
		return true;
	}

	protected override void OnClick()
	{
		Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
		((MainTabWindow_Research)MainButtonDefOf.Research.TabWindow).CurTab = ResearchTabDefOf.Main;
	}
}
