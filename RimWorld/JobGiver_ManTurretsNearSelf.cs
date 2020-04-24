using Verse;

namespace RimWorld
{
	public class JobGiver_ManTurretsNearSelf : JobGiver_ManTurrets
	{
		protected override IntVec3 GetRoot(Pawn pawn)
		{
			return pawn.Position;
		}
	}
}
