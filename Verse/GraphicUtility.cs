using System;
using RimWorld;

namespace Verse
{
	public static class GraphicUtility
	{
		public static Graphic ExtractInnerGraphicFor(this Graphic outerGraphic, Thing thing)
		{
			Graphic_Random graphic_Random = outerGraphic as Graphic_Random;
			if (graphic_Random != null)
			{
				return graphic_Random.SubGraphicFor(thing);
			}
			Graphic_Appearances graphic_Appearances = outerGraphic as Graphic_Appearances;
			if (graphic_Appearances != null)
			{
				return graphic_Appearances.SubGraphicFor(thing);
			}
			return outerGraphic;
		}

		public static Graphic_Linked WrapLinked(Graphic subGraphic, LinkDrawerType linkDrawerType)
		{
			return linkDrawerType switch
			{
				LinkDrawerType.None => null, 
				LinkDrawerType.Basic => new Graphic_Linked(subGraphic), 
				LinkDrawerType.CornerFiller => new Graphic_LinkedCornerFiller(subGraphic), 
				LinkDrawerType.Transmitter => new Graphic_LinkedTransmitter(subGraphic), 
				LinkDrawerType.TransmitterOverlay => new Graphic_LinkedTransmitterOverlay(subGraphic), 
				_ => throw new ArgumentException(), 
			};
		}
	}
}
