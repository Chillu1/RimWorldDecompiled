using Verse;

namespace RimWorld
{
	public class CompProperties_HasSources : CompProperties
	{
		public bool affectLabel = true;

		[MustTranslate]
		public string inspectStringLabel;

		public CompProperties_HasSources()
		{
			compClass = typeof(CompHasSources);
		}
	}
}
