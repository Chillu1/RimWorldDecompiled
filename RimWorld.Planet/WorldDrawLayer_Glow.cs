using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_Glow : WorldDrawLayer
{
	private Material material;

	private const int SubdivisionsCount = 4;

	public const float GlowRadius = 16f;

	public override IEnumerable Regenerate()
	{
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		if (material == null)
		{
			material = new Material(ShaderDatabase.PlanetGlow);
			material.SetFloat(ShaderPropertyIDs.GlowRadius, 16f);
			material.SetFloat(ShaderPropertyIDs.PlanetRadius, planetLayer.Radius);
			material.SetVector(ShaderPropertyIDs.PlanetOrigin, planetLayer.Origin);
		}
		SphereGenerator.Generate(4, planetLayer.Radius + 16f + 0.1f, Vector3.forward, 180f, out var outVerts, out var outIndices);
		LayerSubMesh subMesh = GetSubMesh(material);
		subMesh.verts.AddRange(outVerts);
		subMesh.tris.AddRange(outIndices);
		FinalizeMesh(MeshParts.All);
	}
}
