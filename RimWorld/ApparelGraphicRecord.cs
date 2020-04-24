using Verse;

namespace RimWorld
{
	public struct ApparelGraphicRecord
	{
		public Graphic graphic;

		public Apparel sourceApparel;

		public ApparelGraphicRecord(Graphic graphic, Apparel sourceApparel)
		{
			this.graphic = graphic;
			this.sourceApparel = sourceApparel;
		}
	}
}
