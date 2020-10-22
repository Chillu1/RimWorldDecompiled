using Verse;

namespace RimWorld
{
	public class CompProperties_DestroyAfterDelay : CompProperties
	{
		public int delayTicks;

		public DestroyMode destroyMode;

		[MustTranslate]
		public string countdownLabel;

		public CompProperties_DestroyAfterDelay()
		{
			compClass = typeof(CompDestroyAfterDelay);
		}
	}
}
