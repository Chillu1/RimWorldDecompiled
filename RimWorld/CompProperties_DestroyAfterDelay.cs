using Verse;

namespace RimWorld
{
	public class CompProperties_DestroyAfterDelay : CompProperties
	{
		public int delayTicks;

		public CompProperties_DestroyAfterDelay()
		{
			compClass = typeof(CompDestroyAfterDelay);
		}
	}
}
