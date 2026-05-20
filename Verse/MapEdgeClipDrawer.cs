using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public static class MapEdgeClipDrawer
{
	public static readonly Material ClipMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.1f, 0.1f, 0.1f), ShaderDatabase.MetaOverlay);

	public static readonly Material ClipMatMetalhell = SolidColorMaterials.NewSolidColorMaterial(new Color(0.03f, 0.04f, 0.04f), ShaderDatabase.MetaOverlay);

	private static readonly float ClipAltitude = AltitudeLayer.WorldClipper.AltitudeFor();

	private const float ClipSize = 500f;

	private static MaterialPropertyBlock vertPropertyBlock = new MaterialPropertyBlock();

	private static MaterialPropertyBlock horPropertyBlock = new MaterialPropertyBlock();

	public static void DrawClippers(Map map)
	{
		if (map.DrawMapClippers)
		{
			Material mapEdgeMaterial = map.MapEdgeMaterial;
			IntVec3 size = map.Size;
			Vector3 vector = new Vector3(500f, 1f, size.z);
			Vector3 vector2 = new Vector3(-250f, 0f, (float)size.z / 2f);
			horPropertyBlock.SetVector(ShaderPropertyIDs.MainTextureScale, vector);
			horPropertyBlock.SetVector(ShaderPropertyIDs.MainTextureOffset, vector2);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(vector2.WithYOffset(ClipAltitude), Quaternion.identity, vector);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mapEdgeMaterial, 0, null, 0, horPropertyBlock);
			vector2 = new Vector3((float)size.x + 250f, 0f, (float)size.z / 2f);
			horPropertyBlock.SetVector(ShaderPropertyIDs.MainTextureOffset, vector2);
			matrix = default(Matrix4x4);
			matrix.SetTRS(vector2.WithYOffset(ClipAltitude), Quaternion.identity, vector);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mapEdgeMaterial, 0, null, 0, horPropertyBlock);
			vector = new Vector3(1000f, 1f, 500f);
			vector2 = new Vector3((float)size.x / 2f, 0f, (float)size.z + 250f);
			vertPropertyBlock.SetVector(ShaderPropertyIDs.MainTextureScale, vector);
			vertPropertyBlock.SetVector(ShaderPropertyIDs.MainTextureOffset, vector2);
			matrix = default(Matrix4x4);
			matrix.SetTRS(vector2.WithYOffset(ClipAltitude), Quaternion.identity, vector);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mapEdgeMaterial, 0, null, 0, vertPropertyBlock);
			vector2 = new Vector3((float)size.x / 2f, 0f, -250f);
			vertPropertyBlock.SetVector(ShaderPropertyIDs.MainTextureOffset, vector2);
			matrix = default(Matrix4x4);
			matrix.SetTRS(vector2.WithYOffset(ClipAltitude), Quaternion.identity, vector);
			Graphics.DrawMesh(MeshPool.plane10, matrix, mapEdgeMaterial, 0, null, 0, vertPropertyBlock);
		}
	}
}
