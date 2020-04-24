using Verse;

namespace RimWorld.Planet
{
	public static class WorldPawnsUtility
	{
		public static bool IsWorldPawn(this Pawn p)
		{
			return Find.WorldPawns.Contains(p);
		}
	}
}
