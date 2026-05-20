using Verse;
using Verse.AI;

namespace RimWorld
{
	public class StageFailTrigger_TargetPawnUnreachable : StageFailTrigger
	{
		[NoTranslate]
		public string takerId;

		[NoTranslate]
		public string takeeId;

		public override bool Failed(LordJob_Ritual ritual, TargetInfo spot, TargetInfo focus)
		{
			Pawn pawn = ritual.PawnWithRole(takerId);
			Pawn pawn2 = ritual.PawnWithRole(takeeId);
			return !pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref takerId, "takerId");
			Scribe_Values.Look(ref takeeId, "takeeId");
		}
	}
}
