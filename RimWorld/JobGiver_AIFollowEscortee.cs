using Verse;

namespace RimWorld
{
	public class JobGiver_AIFollowEscortee : JobGiver_AIFollowPawn
	{
		protected override int FollowJobExpireInterval => 120;

		protected override Pawn GetFollowee(Pawn pawn)
		{
			return (Pawn)pawn.mindState.duty.focus.Thing;
		}

		protected override float GetRadius(Pawn pawn)
		{
			return pawn.mindState.duty.radius;
		}
	}
}
