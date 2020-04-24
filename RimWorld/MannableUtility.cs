using Verse;

namespace RimWorld
{
	public static class MannableUtility
	{
		public static Thing MannedThing(this Pawn pawn)
		{
			if (pawn.Dead)
			{
				return null;
			}
			Thing lastMannedThing = pawn.mindState.lastMannedThing;
			if (lastMannedThing == null || lastMannedThing.TryGetComp<CompMannable>().ManningPawn != pawn)
			{
				return null;
			}
			return lastMannedThing;
		}
	}
}
