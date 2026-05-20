using UnityEngine;

namespace Verse;

public class Graphic_StackCount : Graphic_Collection
{
	public override Material MatSingle => subGraphics[subGraphics.Length - 1].MatSingle;

	public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
	{
		return GraphicDatabase.Get<Graphic_StackCount>(path, newShader, drawSize, newColor, newColorTwo, data);
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

	public virtual Graphic SubGraphicFor(Thing thing)
	{
		return SubGraphicForStackCount(thing.stackCount, thing.def);
	}

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		Graphic graphic = ((thing == null) ? subGraphics[0] : SubGraphicFor(thing));
		graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
	}

	public Graphic SubGraphicForStackCount(int stackCount, ThingDef def)
	{
		switch (subGraphics.Length)
		{
		case 1:
			return subGraphics[0];
		case 2:
			if (stackCount == 1)
			{
				return subGraphics[0];
			}
			return subGraphics[1];
		case 3:
			if (stackCount == 1)
			{
				return subGraphics[0];
			}
			if (stackCount == def.stackLimit)
			{
				return subGraphics[2];
			}
			return subGraphics[1];
		default:
		{
			if (stackCount == 1)
			{
				return subGraphics[0];
			}
			if (stackCount == def.stackLimit)
			{
				return subGraphics[subGraphics.Length - 1];
			}
			int num = Mathf.Min(1 + Mathf.RoundToInt((float)stackCount / (float)def.stackLimit * ((float)subGraphics.Length - 3f) + 1E-05f), subGraphics.Length - 2);
			return subGraphics[num];
		}
		}
	}

	public override string ToString()
	{
		return "StackCount(path=" + path + ", count=" + subGraphics.Length + ")";
	}
}
