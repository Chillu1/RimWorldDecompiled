using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class ScenPart_StartingResearch : ScenPart
{
	private ResearchProjectDef project;

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		if (!Widgets.ButtonText(listing.GetScenPartRect(this, ScenPart.RowHeight), project.LabelCap))
		{
			return;
		}
		FloatMenuUtility.MakeMenu(NonRedundantResearchProjects(), (ResearchProjectDef d) => d.LabelCap, (ResearchProjectDef d) => delegate
		{
			project = d;
		});
	}

	public override void Randomize()
	{
		project = NonRedundantResearchProjects().RandomElement();
	}

	private IEnumerable<ResearchProjectDef> NonRedundantResearchProjects()
	{
		return DefDatabase<ResearchProjectDef>.AllDefs.Where((ResearchProjectDef d) => d.tags == null || Find.Scenario.playerFaction.factionDef.startingResearchTags == null || !d.tags.Any((ResearchProjectTagDef tag) => Find.Scenario.playerFaction.factionDef.startingResearchTags.Contains(tag)));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref project, "project");
	}

	public override string Summary(Scenario scen)
	{
		return "ScenPart_StartingResearchFinished".Translate(project.LabelCap);
	}

	public override bool HasNullDefs()
	{
		if (!base.HasNullDefs())
		{
			return project == null;
		}
		return true;
	}

	public override void PostGameStart()
	{
		if (project != null)
		{
			Find.ResearchManager.FinishProject(project, doCompletionDialog: false, null, doCompletionLetter: false);
		}
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ ((project != null) ? project.GetHashCode() : 0);
	}
}
