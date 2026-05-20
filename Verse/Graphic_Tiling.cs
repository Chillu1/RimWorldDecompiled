using UnityEngine;

namespace Verse;

public class Graphic_Tiling : Graphic_WithPropertyBlock
{
	public Vector2 Tiling;

	public Graphic_Tiling WithTiling(Vector2 tiling)
	{
		Tiling = tiling;
		return this;
	}

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		propertyBlock.SetVector(ShaderPropertyIDs.Tiling, Tiling);
		base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
	}
}
