using Verse;

namespace RimWorld
{
	public class CompProperties_WasteProducer : CompProperties
	{
		public bool showContentsInInspectPane = true;

		public CompProperties_WasteProducer()
		{
			compClass = typeof(CompWasteProducer);
		}
	}
}
