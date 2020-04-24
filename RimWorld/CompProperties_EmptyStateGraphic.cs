using Verse;

namespace RimWorld
{
	public class CompProperties_EmptyStateGraphic : CompProperties
	{
		public GraphicData graphicData;

		public CompProperties_EmptyStateGraphic()
		{
			compClass = typeof(CompEmptyStateGraphic);
		}
	}
}
