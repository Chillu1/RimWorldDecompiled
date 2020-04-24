using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class MapEdgeClipDrawer
	{
		public static readonly Material ClipMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.1f, 0.1f, 0.1f), ShaderDatabase.MetaOverlay);

		private static readonly float ClipAltitude = AltitudeLayer.WorldClipper.AltitudeFor();

		private const float ClipWidth = 500f;

		public static void DrawClippers(Map map)
		{
			IntVec3 size = map.Size;
			Vector3 s = new Vector3(500f, 1f, size.z);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(new Vector3(-250f, ClipAltitude, (float)size.z / 2f), Quaternion.identity, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, ClipMat, 0);
			matrix = default(Matrix4x4);
			matrix.SetTRS(new Vector3((float)size.x + 250f, ClipAltitude, (float)size.z / 2f), Quaternion.identity, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, ClipMat, 0);
			s = new Vector3(1000f, 1f, 500f);
			matrix = default(Matrix4x4);
			matrix.SetTRS(new Vector3(size.x / 2, ClipAltitude, (float)size.z + 250f), Quaternion.identity, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, ClipMat, 0);
			matrix = default(Matrix4x4);
			matrix.SetTRS(new Vector3(size.x / 2, ClipAltitude, -250f), Quaternion.identity, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, ClipMat, 0);
		}
	}
}
