using RimWorld;
using UnityEngine;

namespace Verse;

public class Graphic_ActivityStaged : Graphic_Collection
{
	public override Material MatSingle => subGraphics[0].MatSingle;

	public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
	{
		return GraphicDatabase.Get<Graphic_ActivityStaged>(path, newShader, drawSize, newColor, newColorTwo, data);
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

	public override void Print(SectionLayer layer, Thing thing, float extraRotation)
	{
		((thing != null) ? SubGraphicFor(thing) : subGraphics[0]).Print(layer, thing, extraRotation);
		if (base.ShadowGraphic != null && thing != null)
		{
			base.ShadowGraphic.Print(layer, thing, extraRotation);
		}
	}

	private Graphic SubGraphicFor(Thing thing)
	{
		if (thing == null)
		{
			return subGraphics[0];
		}
		if (!thing.TryGetComp(out CompActivity comp))
		{
			Log.ErrorOnce(thing.Label + ": Graphic_ActivityStaged requires CompActivity.", 4627811);
			return null;
		}
		int num = Mathf.Min(Mathf.FloorToInt((float)subGraphics.Length * comp.ActivityLevel), subGraphics.Length - 1);
		return subGraphics[num];
	}

	public override string ToString()
	{
		return "ActivityStaged(path=" + path + ", count=" + subGraphics.Length + ")";
	}
}
