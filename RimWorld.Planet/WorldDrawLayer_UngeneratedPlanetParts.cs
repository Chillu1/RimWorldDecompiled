using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_UngeneratedPlanetParts : WorldDrawLayer
{
	private const int SubdivisionsCount = 4;

	private const float ViewAngleOffset = 10f;

	public override IEnumerable Regenerate()
	{
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		Vector3 surfaceViewCenter = Find.WorldGrid.SurfaceViewCenter;
		float surfaceViewAngle = Find.WorldGrid.SurfaceViewAngle;
		if (surfaceViewAngle < 180f)
		{
			SphereGenerator.Generate(4, planetLayer.Radius + -0.16f, -surfaceViewCenter, 180f - Mathf.Min(surfaceViewAngle, 180f) + 10f, out var outVerts, out var outIndices);
			LayerSubMesh subMesh = GetSubMesh(WorldMaterials.UngeneratedPlanetParts);
			subMesh.verts.AddRange(outVerts);
			subMesh.tris.AddRange(outIndices);
		}
		FinalizeMesh(MeshParts.All);
	}
}
