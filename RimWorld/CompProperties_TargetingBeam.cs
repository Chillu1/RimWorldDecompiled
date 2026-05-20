using Verse;

namespace RimWorld
{
	public class CompProperties_TargetingBeam : CompProperties
	{
		public int delayTicks;

		public DamageDef damage;

		public CompProperties_TargetingBeam()
		{
			compClass = typeof(CompTargetingBeam);
		}
	}
}
