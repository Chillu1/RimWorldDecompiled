using Verse;

namespace RimWorld
{
	public class CompProperties_Initiatable : CompProperties
	{
		public int initiationDelayTicks;

		public CompProperties_Initiatable()
		{
			compClass = typeof(CompInitiatable);
		}
	}
}
