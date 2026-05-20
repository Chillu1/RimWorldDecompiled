using Verse;

namespace RimWorld
{
	public class CompProperties_HasPawnSources : CompProperties
	{
		public bool affectLabel = true;

		public CompProperties_HasPawnSources()
		{
			compClass = typeof(CompHasPawnSources);
		}
	}
}
