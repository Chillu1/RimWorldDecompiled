using RimWorld;
using Verse;

public static class ResearchUtility
{
	public static void ApplyPlayerStartingResearch()
	{
		if (Faction.OfPlayer.def.startingResearchTags != null)
		{
			foreach (ResearchProjectTagDef startingResearchTag in Faction.OfPlayer.def.startingResearchTags)
			{
				foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
				{
					if (allDef.HasTag(startingResearchTag))
					{
						Find.ResearchManager.FinishProject(allDef, doCompletionDialog: false, null, doCompletionLetter: false);
					}
				}
			}
		}
		Ideo ideo;
		if (ModLister.IdeologyInstalled && (ideo = Faction.OfPlayer.ideos?.PrimaryIdeo) != null)
		{
			foreach (MemeDef meme in ideo.memes)
			{
				foreach (ResearchProjectDef startingResearchProject in meme.startingResearchProjects)
				{
					Find.ResearchManager.FinishProject(startingResearchProject, doCompletionDialog: false, null, doCompletionLetter: false);
				}
			}
		}
		if (Faction.OfPlayer.def.startingTechprintsResearchTags == null)
		{
			return;
		}
		foreach (ResearchProjectTagDef startingTechprintsResearchTag in Faction.OfPlayer.def.startingTechprintsResearchTags)
		{
			foreach (ResearchProjectDef allDef2 in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				if (allDef2.HasTag(startingTechprintsResearchTag))
				{
					int techprints = Find.ResearchManager.GetTechprints(allDef2);
					if (techprints < allDef2.TechprintCount)
					{
						Find.ResearchManager.AddTechprints(allDef2, allDef2.TechprintCount - techprints);
					}
				}
			}
		}
	}
}
