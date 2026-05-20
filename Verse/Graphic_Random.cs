using UnityEngine;

namespace Verse;

public class Graphic_Random : Graphic_Collection
{
	public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

	public int SubGraphicsCount => subGraphics.Length;

	public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
	{
		if (newColorTwo != Color.white)
		{
			Log.ErrorOnce("Cannot use Graphic_Random.GetColoredVersion with a non-white colorTwo.", 9910251);
		}
		return GraphicDatabase.Get<Graphic_Random>(path, newShader, drawSize, newColor, Color.white, data);
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
		if (thing == null)
		{
			return MatSingle;
		}
		return SubGraphicFor(thing).MatSingle;
	}

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		((thing != null) ? SubGraphicFor(thing) : subGraphics[0]).DrawWorker(loc, rot, thingDef, thing, extraRotation);
		if (base.ShadowGraphic != null)
		{
			base.ShadowGraphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
		}
	}

	public Graphic SubGraphicFor(Thing thing)
	{
		if (thing == null)
		{
			return subGraphics[0];
		}
		int num = thing.OverrideGraphicIndex ?? thing.thingIDNumber;
		return subGraphics[num % subGraphics.Length];
	}

	public Graphic SubGraphicAtIndex(int index)
	{
		return subGraphics[index % subGraphics.Length];
	}

	public Graphic FirstSubgraphic()
	{
		return subGraphics[0];
	}

	public override void Print(SectionLayer layer, Thing thing, float extraRotation)
	{
		((thing != null) ? SubGraphicFor(thing) : subGraphics[0]).Print(layer, thing, extraRotation);
		if (base.ShadowGraphic != null && thing != null)
		{
			base.ShadowGraphic.Print(layer, thing, extraRotation);
		}
	}

	public override string ToString()
	{
		return "Random(path=" + path + ", count=" + subGraphics.Length + ")";
	}
}
