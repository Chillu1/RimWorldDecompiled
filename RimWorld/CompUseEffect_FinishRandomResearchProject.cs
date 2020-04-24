using Verse;

namespace RimWorld
{
	public class CompUseEffect_FinishRandomResearchProject : CompUseEffect
	{
		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj != null)
			{
				FinishInstantly(currentProj, usedBy);
			}
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if (Find.ResearchManager.currentProj == null)
			{
				failReason = "NoActiveResearchProjectToFinish".Translate();
				return false;
			}
			failReason = null;
			return true;
		}

		private void FinishInstantly(ResearchProjectDef proj, Pawn usedBy)
		{
			Find.ResearchManager.FinishProject(proj);
			Messages.Message("MessageResearchProjectFinishedByItem".Translate(proj.LabelCap), usedBy, MessageTypeDefOf.PositiveEvent);
		}
	}
}
