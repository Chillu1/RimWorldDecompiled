using RimWorld;
using UnityEngine;

namespace Verse;

public class Graphic_Genepack : Graphic_Collection
{
	private const int MaxDisplayedGenes = 4;

	public override Material MatSingle => subGraphics[0].MatSingle;

	public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
	{
		return GraphicDatabase.Get<Graphic_Genepack>(path, newShader, drawSize, newColor, newColorTwo, data);
	}

	public Graphic SubGraphicFor(Thing thing)
	{
		if (thing == null || !(thing is Genepack { GeneSet: not null } genepack))
		{
			return subGraphics[0];
		}
		foreach (GeneDef item in genepack.GeneSet.GenesListForReading)
		{
			if (item.biostatArc > 0)
			{
				return subGraphics[subGraphics.Length - 1];
			}
		}
		return SubGraphicForGeneCount(genepack.GeneSet.GenesListForReading.Count);
	}

	public override Material MatAt(Rot4 rot, Thing thing = null)
	{
		if (thing == null)
		{
			return MatSingle;
		}
		return MatSingleFor(thing);
	}

	public override Material MatSingleFor(Thing thing)
	{
		return SubGraphicFor(thing).MatSingle;
	}

	public Graphic SubGraphicForGeneCount(int geneCount)
	{
		geneCount = Mathf.Min(geneCount, 4);
		return subGraphics[Mathf.Min(geneCount, subGraphics.Length - 1)];
	}
}
