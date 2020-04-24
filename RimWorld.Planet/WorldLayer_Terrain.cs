using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Terrain : WorldLayer
	{
		private List<MeshCollider> meshCollidersInOrder = new List<MeshCollider>();

		private List<List<int>> triangleIndexToTileID = new List<List<int>>();

		private List<Vector3> elevationValues = new List<Vector3>();

		public override IEnumerable Regenerate()
		{
			foreach (object item in base.Regenerate())
			{
				yield return item;
			}
			WorldGrid grid = Find.World.grid;
			int tilesCount = grid.TilesCount;
			List<Tile> tiles = grid.tiles;
			List<int> tileIDToVerts_offsets = grid.tileIDToVerts_offsets;
			List<Vector3> verts = grid.verts;
			triangleIndexToTileID.Clear();
			foreach (object item2 in CalculateInterpolatedVerticesParams())
			{
				yield return item2;
			}
			int num = 0;
			for (int i = 0; i < tilesCount; i++)
			{
				BiomeDef biome = tiles[i].biome;
				int subMeshIndex;
				LayerSubMesh subMesh = GetSubMesh(biome.DrawMaterial, out subMeshIndex);
				while (subMeshIndex >= triangleIndexToTileID.Count)
				{
					triangleIndexToTileID.Add(new List<int>());
				}
				int count = subMesh.verts.Count;
				int num2 = 0;
				int num3 = (i + 1 < tileIDToVerts_offsets.Count) ? tileIDToVerts_offsets[i + 1] : verts.Count;
				for (int j = tileIDToVerts_offsets[i]; j < num3; j++)
				{
					subMesh.verts.Add(verts[j]);
					subMesh.uvs.Add(elevationValues[num]);
					num++;
					if (j < num3 - 2)
					{
						subMesh.tris.Add(count + num2 + 2);
						subMesh.tris.Add(count + num2 + 1);
						subMesh.tris.Add(count);
						triangleIndexToTileID[subMeshIndex].Add(i);
					}
					num2++;
				}
			}
			FinalizeMesh(MeshParts.All);
			foreach (object item3 in RegenerateMeshColliders())
			{
				yield return item3;
			}
			elevationValues.Clear();
			elevationValues.TrimExcess();
		}

		public int GetTileIDFromRayHit(RaycastHit hit)
		{
			int i = 0;
			for (int count = meshCollidersInOrder.Count; i < count; i++)
			{
				if (meshCollidersInOrder[i] == hit.collider)
				{
					return triangleIndexToTileID[i][hit.triangleIndex];
				}
			}
			return -1;
		}

		private IEnumerable RegenerateMeshColliders()
		{
			meshCollidersInOrder.Clear();
			GameObject gameObject = WorldTerrainColliderManager.GameObject;
			MeshCollider[] components = gameObject.GetComponents<MeshCollider>();
			for (int j = 0; j < components.Length; j++)
			{
				Object.Destroy(components[j]);
			}
			for (int i = 0; i < subMeshes.Count; i++)
			{
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.sharedMesh = subMeshes[i].mesh;
				meshCollidersInOrder.Add(meshCollider);
				yield return null;
			}
		}

		private IEnumerable CalculateInterpolatedVerticesParams()
		{
			elevationValues.Clear();
			WorldGrid grid = Find.World.grid;
			int tilesCount = grid.TilesCount;
			List<Vector3> verts = grid.verts;
			List<int> tileIDToVerts_offsets = grid.tileIDToVerts_offsets;
			List<int> tileIDToNeighbors_offsets = grid.tileIDToNeighbors_offsets;
			List<int> tileIDToNeighbors_values = grid.tileIDToNeighbors_values;
			List<Tile> tiles = grid.tiles;
			for (int i = 0; i < tilesCount; i++)
			{
				Tile tile = tiles[i];
				float elevation = tile.elevation;
				int num = (i + 1 < tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_offsets[i + 1] : tileIDToNeighbors_values.Count;
				int num2 = (i + 1 < tilesCount) ? tileIDToVerts_offsets[i + 1] : verts.Count;
				for (int j = tileIDToVerts_offsets[i]; j < num2; j++)
				{
					Vector3 item = default(Vector3);
					item.x = elevation;
					bool flag = false;
					for (int k = tileIDToNeighbors_offsets[i]; k < num; k++)
					{
						int num3 = (tileIDToNeighbors_values[k] + 1 < tileIDToVerts_offsets.Count) ? tileIDToVerts_offsets[tileIDToNeighbors_values[k] + 1] : verts.Count;
						for (int l = tileIDToVerts_offsets[tileIDToNeighbors_values[k]]; l < num3; l++)
						{
							if (!(verts[l] == verts[j]))
							{
								continue;
							}
							Tile tile2 = tiles[tileIDToNeighbors_values[k]];
							if (!flag)
							{
								if ((tile2.elevation >= 0f && elevation <= 0f) || (tile2.elevation <= 0f && elevation >= 0f))
								{
									flag = true;
								}
								else if (tile2.elevation > item.x)
								{
									item.x = tile2.elevation;
								}
							}
							break;
						}
					}
					if (flag)
					{
						item.x = 0f;
					}
					if (tile.biome.DrawMaterial.shader != ShaderDatabase.WorldOcean && item.x < 0f)
					{
						item.x = 0f;
					}
					elevationValues.Add(item);
				}
				if (i % 1000 == 0)
				{
					yield return null;
				}
			}
		}
	}
}
