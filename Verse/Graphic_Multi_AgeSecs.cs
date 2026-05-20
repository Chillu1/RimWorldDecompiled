using UnityEngine;

namespace Verse;

public class Graphic_Multi_AgeSecs : Graphic_Multi
{
	public MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

	public float AgeSecs(Thing thing)
	{
		return (float)(Find.TickManager.TicksGame - thing.TickSpawned) / 60f;
	}

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		Material material = MatAt(rot, thing);
		if (thing != null)
		{
			material.SetFloat(ShaderPropertyIDs.AgeSecs, AgeSecs(thing));
		}
		base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
	}

	protected override void DrawMeshInt(Mesh mesh, Vector3 loc, Quaternion quat, Material mat)
	{
		Graphics.DrawMesh(mesh, loc, quat, mat, 0, null, 0, propertyBlock);
	}
}
