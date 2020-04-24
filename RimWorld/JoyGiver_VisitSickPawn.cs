using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_VisitSickPawn : JoyGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			if (!InteractionUtility.CanInitiateInteraction(pawn))
			{
				return null;
			}
			Pawn pawn2 = SickPawnVisitUtility.FindRandomSickPawn(pawn, JoyCategory.Low);
			if (pawn2 == null)
			{
				return null;
			}
			return JobMaker.MakeJob(def.jobDef, pawn2, SickPawnVisitUtility.FindChair(pawn, pawn2));
		}
	}
}
