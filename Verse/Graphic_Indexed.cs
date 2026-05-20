using UnityEngine;

namespace Verse;

public class Graphic_Indexed : Graphic_Collection
{
	public override Material MatSingle => subGraphics[0].MatSingle;

	public int SubGraphicsCount => subGraphics.Length;

	public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
	{
		if (newColorTwo != Color.white)
		{
			Log.ErrorOnce("Cannot use Graphic_Indexed.GetColoredVersion with a non-white colorTwo.", 9910251);
		}
		return GraphicDatabase.Get<Graphic_Indexed>(path, newShader, drawSize, newColor, Color.white, data);
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
		Graphic graphic = ((thing == null) ? subGraphics[0] : SubGraphicFor(thing));
		graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
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
		int valueOrDefault = thing.OverrideGraphicIndex.GetValueOrDefault();
		return subGraphics[valueOrDefault % subGraphics.Length];
	}

	public Graphic SubGraphicAtIndex(int index)
	{
		return subGraphics[index % subGraphics.Length];
	}

	public Graphic FirstSubgraphic()
	{
		return subGraphics[0];
	}

	public override string ToString()
	{
		return "Indexed(path=" + path + ", count=" + subGraphics.Length + ")";
	}
}
