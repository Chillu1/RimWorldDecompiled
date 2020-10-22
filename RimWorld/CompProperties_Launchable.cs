using Verse;

namespace RimWorld
{
	public class CompProperties_Launchable : CompProperties
	{
		public bool requireFuel = true;

		public int fixedLaunchDistanceMax = -1;

		public ThingDef skyfallerLeaving;

		public CompProperties_Launchable()
		{
			compClass = typeof(CompLaunchable);
		}
	}
}
