using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ScenPart_StartingResearch : ScenPart
	{
		private ResearchProjectDef project;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			if (Widgets.ButtonText(listing.GetScenPartRect(this, ScenPart.RowHeight), project.LabelCap))
			{
				FloatMenuUtility.MakeMenu(NonRedundantResearchProjects(), (ResearchProjectDef d) => d.LabelCap, delegate(ResearchProjectDef d)
				{
					ScenPart_StartingResearch scenPart_StartingResearch = this;
					return delegate
					{
						scenPart_StartingResearch.project = d;
					};
				});
			}
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

		public override void PostGameStart()
		{
			Find.ResearchManager.FinishProject(project);
		}
	}
}
