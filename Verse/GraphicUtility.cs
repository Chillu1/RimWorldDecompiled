using System;
using RimWorld;

namespace Verse;

public static class GraphicUtility
{
	public static Graphic ExtractInnerGraphicFor(this Graphic outerGraphic, Thing thing, int? indexOverride = null)
	{
		if (outerGraphic is Graphic_RandomRotated graphic_RandomRotated)
		{
			return ResolveGraphicInner(graphic_RandomRotated.SubGraphic);
		}
		return ResolveGraphicInner(outerGraphic);
		Graphic ResolveGraphicInner(Graphic g)
		{
			if (g is Graphic_Random graphic_Random)
			{
				if (indexOverride.HasValue)
				{
					return graphic_Random.SubGraphicAtIndex(indexOverride.Value);
				}
				return graphic_Random.SubGraphicFor(thing);
			}
			if (g is Graphic_Cluster graphic_Cluster)
			{
				return graphic_Cluster.SubGraphicFor(thing);
			}
			if (g is Graphic_Appearances graphic_Appearances)
			{
				return graphic_Appearances.SubGraphicFor(thing);
			}
			if (g is Graphic_Genepack graphic_Genepack)
			{
				return graphic_Genepack.SubGraphicFor(thing);
			}
			if (g is Graphic_MealVariants graphic_MealVariants)
			{
				return graphic_MealVariants.SubGraphicFor(thing);
			}
			return g;
		}
	}

	public static Graphic_Linked WrapLinked(Graphic subGraphic, LinkDrawerType linkDrawerType)
	{
		return linkDrawerType switch
		{
			LinkDrawerType.None => null, 
			LinkDrawerType.Basic => new Graphic_Linked(subGraphic), 
			LinkDrawerType.CornerFiller => new Graphic_LinkedCornerFiller(subGraphic), 
			LinkDrawerType.CornerOverlay => new Graphic_LinkedCornerOverlay(subGraphic), 
			LinkDrawerType.Transmitter => new Graphic_LinkedTransmitter(subGraphic), 
			LinkDrawerType.TransmitterOverlay => new Graphic_LinkedTransmitterOverlay(subGraphic), 
			LinkDrawerType.Asymmetric => new Graphic_LinkedAsymmetric(subGraphic), 
			_ => throw new ArgumentException(), 
		};
	}
}
