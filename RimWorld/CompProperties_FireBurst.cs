using Verse;

namespace RimWorld
{
	public class CompProperties_FireBurst : CompProperties
	{
		public float radius = 6f;

		public int ticksAwayFromDetonate = 17;

		public CompProperties_FireBurst()
		{
			compClass = typeof(CompFireBurst);
		}
	}
}
