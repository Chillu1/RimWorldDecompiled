using Verse;

namespace RimWorld
{
	public class JobGiver_AIDefendEscortee : JobGiver_AIDefendPawn
	{
		protected override Pawn GetDefendee(Pawn pawn)
		{
			return ((Thing)pawn.mindState.duty.focus) as Pawn;
		}

		protected override float GetFlagRadius(Pawn pawn)
		{
			return pawn.mindState.duty.radius;
		}
	}
}
