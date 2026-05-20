using UnityEngine;

namespace Verse;

public abstract class Graphic_WithPropertyBlock : Graphic_Single
{
	public MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

	protected override void DrawMeshInt(Mesh mesh, Vector3 loc, Quaternion quat, Material mat)
	{
		Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(loc, quat, new Vector3(drawSize.x, 1f, drawSize.y)), mat, 0, null, 0, propertyBlock);
	}
}
