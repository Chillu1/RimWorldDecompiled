using Verse;

namespace RimWorld
{
	public class CompProperties_OverseerSubject : CompProperties
	{
		public EffecterDef needsOverseerEffect;

		public int delayUntilFeralCheck = 60000;

		public int feralMtbDays = 10;

		public int feralCascadeRadialDistance = 25;

		public CompProperties_OverseerSubject()
		{
			compClass = typeof(CompOverseerSubject);
		}
	}
}
