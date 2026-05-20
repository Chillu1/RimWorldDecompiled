using Verse;

namespace RimWorld
{
	public class CompProperties_InspectString : CompProperties
	{
		[MustTranslate]
		public string inspectString;

		public CompProperties_InspectString()
		{
			compClass = typeof(CompInspectString);
		}
	}
}
