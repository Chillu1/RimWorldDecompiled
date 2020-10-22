using System.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Glow : WorldLayer
	{
		private const int SubdivisionsCount = 4;

		public const float GlowRadius = 8f;

		public override IEnumerable Regenerate()
		{
			foreach (object item in base.Regenerate())
			{
				yield return item;
			}
			SphereGenerator.Generate(4, 108.1f, Vector3.forward, 360f, out var outVerts, out var outIndices);
			LayerSubMesh subMesh = GetSubMesh(WorldMaterials.PlanetGlow);
			subMesh.verts.AddRange(outVerts);
			subMesh.tris.AddRange(outIndices);
			FinalizeMesh(MeshParts.All);
		}
	}
}
