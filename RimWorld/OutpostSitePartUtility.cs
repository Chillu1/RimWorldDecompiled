using RimWorld.Planet;

namespace RimWorld
{
	public static class OutpostSitePartUtility
	{
		public static int GetPawnGroupMakerSeed(SitePartParams parms)
		{
			return parms.randomValue;
		}
	}
}
