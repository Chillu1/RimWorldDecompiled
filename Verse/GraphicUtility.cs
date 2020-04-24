using RimWorld;
using System;

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
			return outerGraphic;
		}

		public static Graphic_Linked WrapLinked(Graphic subGraphic, LinkDrawerType linkDrawerType)
		{
			switch (linkDrawerType)
			{
			case LinkDrawerType.None:
				return null;
			case LinkDrawerType.Basic:
				return new Graphic_Linked(subGraphic);
			case LinkDrawerType.CornerFiller:
				return new Graphic_LinkedCornerFiller(subGraphic);
			case LinkDrawerType.Transmitter:
				return new Graphic_LinkedTransmitter(subGraphic);
			case LinkDrawerType.TransmitterOverlay:
				return new Graphic_LinkedTransmitterOverlay(subGraphic);
			default:
				throw new ArgumentException();
			}
		}
	}
}
