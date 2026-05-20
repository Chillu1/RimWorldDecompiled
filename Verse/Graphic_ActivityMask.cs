using RimWorld;
using UnityEngine;

namespace Verse;

public class Graphic_ActivityMask : Graphic_WithPropertyBlock
{
	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		CompActivity compActivity = thing.TryGetComp<CompActivity>();
		if (compActivity == null)
		{
			Log.ErrorOnce(thingDef.defName + ": Graphic_ActivityMask requires CompActivity.", 6134621);
			return;
		}
		Color value = colorTwo;
		value.a = Mathf.Clamp01(compActivity.ActivityLevel);
		propertyBlock.SetColor(ShaderPropertyIDs.ColorTwo, value);
		base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
	}
}
