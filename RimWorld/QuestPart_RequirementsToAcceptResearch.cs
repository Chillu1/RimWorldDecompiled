using Verse;

namespace RimWorld;

public class QuestPart_RequirementsToAcceptResearch : QuestPart_RequirementsToAccept
{
	public ResearchProjectDef project;

	public override AcceptanceReport CanAccept()
	{
		ResearchProjectDef researchProjectDef = project;
		if (researchProjectDef != null && !researchProjectDef.IsFinished)
		{
			return new AcceptanceReport("QuestRequiresResearch".Translate());
		}
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref project, "project");
	}
}
