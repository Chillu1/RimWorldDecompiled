using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_UngeneratedPlanetParts : WorldLayer
	{
		private const int SubdivisionsCount = 4;

		private const float ViewAngleOffset = 10f;

		public override IEnumerable Regenerate()
		{
			foreach (object item in base.Regenerate())
			{
				yield return item;
			}
			Vector3 viewCenter = Find.WorldGrid.viewCenter;
			float viewAngle = Find.WorldGrid.viewAngle;
			if (viewAngle < 180f)
			{
				SphereGenerator.Generate(4, 99.85f, -viewCenter, 180f - Mathf.Min(viewAngle, 180f) + 10f, out var outVerts, out var outIndices);
				LayerSubMesh subMesh = GetSubMesh(WorldMaterials.UngeneratedPlanetParts);
				subMesh.verts.AddRange(outVerts);
				subMesh.tris.AddRange(outIndices);
			}
			FinalizeMesh(MeshParts.All);
		}
	}
}
