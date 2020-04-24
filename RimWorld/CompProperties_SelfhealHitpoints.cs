using Verse;

namespace RimWorld
{
	public class CompProperties_SelfhealHitpoints : CompProperties
	{
		public int ticksPerHeal;

		public CompProperties_SelfhealHitpoints()
		{
			compClass = typeof(CompSelfhealHitpoints);
		}
	}
}
