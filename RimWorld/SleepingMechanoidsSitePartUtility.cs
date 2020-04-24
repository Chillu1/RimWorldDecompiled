using RimWorld.Planet;

namespace RimWorld
{
	public static class SleepingMechanoidsSitePartUtility
	{
		public static int GetPawnGroupMakerSeed(SitePartParams parms)
		{
			return parms.randomValue;
		}
	}
}
