using Verse;

namespace RimWorld.Planet
{
	public static class WorldReachabilityUtility
	{
		public static bool CanReach(this Caravan c, int tile)
		{
			return Find.WorldReachability.CanReach(c, tile);
		}
	}
}
