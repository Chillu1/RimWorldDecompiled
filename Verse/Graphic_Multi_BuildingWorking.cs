using UnityEngine;

namespace Verse;

public class Graphic_Multi_BuildingWorking : Graphic_Multi_AgeSecs
{
	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		MatAt(rot, thing);
		if (thing is Building building)
		{
			propertyBlock.SetFloat(ShaderPropertyIDs.Working, building.IsWorking() ? 1f : 0f);
		}
		propertyBlock.SetFloat(ShaderPropertyIDs.Rotation, rot.AsInt);
		if (thing != null)
		{
			propertyBlock.SetFloat(ShaderPropertyIDs.RandomPerObject, thing.thingIDNumber.HashOffset());
		}
		base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
	}
}
