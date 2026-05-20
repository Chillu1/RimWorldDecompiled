using Verse;

namespace RimWorld
{
	public class CompProperties_GenepackContainer : CompProperties
	{
		public int maxCapacity;

		public CompProperties_GenepackContainer()
		{
			compClass = typeof(CompGenepackContainer);
		}
	}
}
