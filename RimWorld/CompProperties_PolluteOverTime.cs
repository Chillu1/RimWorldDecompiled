using Verse;

namespace RimWorld
{
	public class CompProperties_PolluteOverTime : CompProperties
	{
		public int cellsToPollutePerDay;

		public CompProperties_PolluteOverTime()
		{
			compClass = typeof(CompPolluteOverTime);
		}
	}
}
