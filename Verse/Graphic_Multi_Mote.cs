using UnityEngine;

namespace Verse;

public class Graphic_Multi_Mote : Graphic_Multi_AgeSecs
{
	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		if (thing is Mote mote)
		{
			propertyBlock.SetColor(ShaderPropertyIDs.Color, mote.instanceColor);
		}
		base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
	}

	protected override void DrawMeshInt(Mesh mesh, Vector3 loc, Quaternion quat, Material mat)
	{
		Graphics.DrawMesh(mesh, loc, quat, mat, 0, null, 0, propertyBlock);
	}
}
