using UnityEngine;

namespace Verse
{
	public class GraphicMeshSet
	{
		private Mesh[] meshes = new Mesh[4];

		public GraphicMeshSet(Mesh normalMesh, Mesh leftMesh)
		{
			meshes[0] = (meshes[1] = (meshes[2] = normalMesh));
			meshes[3] = leftMesh;
		}

		public GraphicMeshSet(float size)
		{
			meshes[0] = (meshes[1] = (meshes[2] = MeshMakerPlanes.NewPlaneMesh(size, flipped: false, backLift: true)));
			meshes[3] = MeshMakerPlanes.NewPlaneMesh(size, flipped: true, backLift: true);
		}

		public GraphicMeshSet(float width, float height)
		{
			Vector2 size = new Vector2(width, height);
			meshes[0] = (meshes[1] = (meshes[2] = MeshMakerPlanes.NewPlaneMesh(size, flipped: false, backLift: true, twist: false)));
			meshes[3] = MeshMakerPlanes.NewPlaneMesh(size, flipped: true, backLift: true, twist: false);
		}

		public Mesh MeshAt(Rot4 rot)
		{
			return meshes[rot.AsInt];
		}
	}
}
