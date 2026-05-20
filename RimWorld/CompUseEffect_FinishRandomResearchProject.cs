using Verse;

namespace RimWorld;

public class CompUseEffect_FinishRandomResearchProject : CompUseEffect
{
	public override void DoEffect(Pawn usedBy)
	{
		base.DoEffect(usedBy);
		ResearchProjectDef project = Find.ResearchManager.GetProject();
		if (project != null)
		{
			FinishInstantly(project, usedBy);
		}
	}

	public override AcceptanceReport CanBeUsedBy(Pawn p)
	{
		if (Find.ResearchManager.GetProject() == null)
		{
			return "NoActiveResearchProjectToFinish".Translate();
		}
		return true;
	}

	private void FinishInstantly(ResearchProjectDef proj, Pawn usedBy)
	{
		Find.ResearchManager.FinishProject(proj, doCompletionDialog: true);
		Messages.Message("MessageResearchProjectFinishedByItem".Translate(proj.LabelCap), usedBy, MessageTypeDefOf.PositiveEvent);
	}
}
