using Verse;

namespace RimWorld
{
	public class CompProperties_SkyfallerRandomizeDirection : CompProperties
	{
		public IntRange directionChangeInterval;

		public float maxDeviationFromStartingAngle;

		public CompProperties_SkyfallerRandomizeDirection()
		{
			compClass = typeof(CompSkyfallerRandomizeDirection);
		}
	}
}
