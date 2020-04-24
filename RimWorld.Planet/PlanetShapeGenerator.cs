using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class PlanetShapeGenerator
	{
		private static int subdivisionsCount;

		private static float radius;

		private static Vector3 viewCenter;

		private static float viewAngle;

		private static List<TriangleIndices> tris = new List<TriangleIndices>();

		private static List<Vector3> verts = new List<Vector3>();

		private static List<Vector3> finalVerts;

		private static List<int> tileIDToFinalVerts_offsets;

		private static List<int> tileIDToNeighbors_offsets;

		private static List<int> tileIDToNeighbors_values;

		private static List<TriangleIndices> newTris = new List<TriangleIndices>();

		private static List<int> generatedTileVerts = new List<int>();

		private static List<int> adjacentTris = new List<int>();

		private static List<int> tmpTileIDs = new List<int>();

		private static List<int> tmpVerts = new List<int>();

		private static List<int> tmpNeighborsToAdd = new List<int>();

		private static List<int> vertToTris_offsets = new List<int>();

		private static List<int> vertToTris_values = new List<int>();

		private static List<int> vertToTileIDs_offsets = new List<int>();

		private static List<int> vertToTileIDs_values = new List<int>();

		private static List<int> tileIDToVerts_offsets = new List<int>();

		private static List<int> tileIDToVerts_values = new List<int>();

		private const int MaxTileVertices = 6;

		public static void Generate(int subdivisionsCount, out List<Vector3> outVerts, out List<int> outTileIDToVerts_offsets, out List<int> outTileIDToNeighbors_offsets, out List<int> outTileIDToNeighbors_values, float radius, Vector3 viewCenter, float viewAngle)
		{
			PlanetShapeGenerator.subdivisionsCount = subdivisionsCount;
			PlanetShapeGenerator.radius = radius;
			PlanetShapeGenerator.viewCenter = viewCenter;
			PlanetShapeGenerator.viewAngle = viewAngle;
			DoGenerate();
			outVerts = finalVerts;
			outTileIDToVerts_offsets = tileIDToFinalVerts_offsets;
			outTileIDToNeighbors_offsets = tileIDToNeighbors_offsets;
			outTileIDToNeighbors_values = tileIDToNeighbors_values;
		}

		private static void DoGenerate()
		{
			ClearOrCreateMeshStaticData();
			CreateTileInfoStaticData();
			IcosahedronGenerator.GenerateIcosahedron(verts, tris, radius, viewCenter, viewAngle);
			for (int i = 0; i < subdivisionsCount + 1; i++)
			{
				Subdivide(i == subdivisionsCount);
			}
			CalculateTileNeighbors();
			ClearAndDeallocateWorkingLists();
		}

		private static void ClearOrCreateMeshStaticData()
		{
			tris.Clear();
			verts.Clear();
			finalVerts = new List<Vector3>();
		}

		private static void CreateTileInfoStaticData()
		{
			tileIDToFinalVerts_offsets = new List<int>();
			tileIDToNeighbors_offsets = new List<int>();
			tileIDToNeighbors_values = new List<int>();
		}

		private static void ClearAndDeallocateWorkingLists()
		{
			ClearAndDeallocate(ref tris);
			ClearAndDeallocate(ref verts);
			ClearAndDeallocate(ref newTris);
			ClearAndDeallocate(ref generatedTileVerts);
			ClearAndDeallocate(ref adjacentTris);
			ClearAndDeallocate(ref tmpTileIDs);
			ClearAndDeallocate(ref tmpVerts);
			ClearAndDeallocate(ref tmpNeighborsToAdd);
			ClearAndDeallocate(ref vertToTris_offsets);
			ClearAndDeallocate(ref vertToTris_values);
			ClearAndDeallocate(ref vertToTileIDs_offsets);
			ClearAndDeallocate(ref vertToTileIDs_values);
			ClearAndDeallocate(ref tileIDToVerts_offsets);
			ClearAndDeallocate(ref tileIDToVerts_values);
		}

		private static void ClearAndDeallocate<T>(ref List<T> list)
		{
			list.Clear();
			list.TrimExcess();
			list = new List<T>();
		}

		private static void Subdivide(bool lastPass)
		{
			PackedListOfLists.GenerateVertToTrisPackedList(verts, tris, vertToTris_offsets, vertToTris_values);
			int count = verts.Count;
			int i = 0;
			for (int count2 = tris.Count; i < count2; i++)
			{
				TriangleIndices triangleIndices = tris[i];
				Vector3 vector = (verts[triangleIndices.v1] + verts[triangleIndices.v2] + verts[triangleIndices.v3]) / 3f;
				verts.Add(vector.normalized * radius);
			}
			newTris.Clear();
			if (lastPass)
			{
				vertToTileIDs_offsets.Clear();
				vertToTileIDs_values.Clear();
				tileIDToVerts_offsets.Clear();
				tileIDToVerts_values.Clear();
				int j = 0;
				for (int count3 = verts.Count; j < count3; j++)
				{
					vertToTileIDs_offsets.Add(vertToTileIDs_values.Count);
					if (j >= count)
					{
						for (int k = 0; k < 6; k++)
						{
							vertToTileIDs_values.Add(-1);
						}
					}
				}
			}
			for (int l = 0; l < count; l++)
			{
				PackedListOfLists.GetList(vertToTris_offsets, vertToTris_values, l, adjacentTris);
				int count4 = adjacentTris.Count;
				if (!lastPass)
				{
					for (int m = 0; m < count4; m++)
					{
						int num = adjacentTris[m];
						int v = count + num;
						int nextOrderedVertex = tris[num].GetNextOrderedVertex(l);
						int num2 = -1;
						for (int n = 0; n < count4; n++)
						{
							if (m != n)
							{
								TriangleIndices triangleIndices2 = tris[adjacentTris[n]];
								if (triangleIndices2.v1 == nextOrderedVertex || triangleIndices2.v2 == nextOrderedVertex || triangleIndices2.v3 == nextOrderedVertex)
								{
									num2 = adjacentTris[n];
									break;
								}
							}
						}
						if (num2 >= 0)
						{
							int v2 = count + num2;
							newTris.Add(new TriangleIndices(l, v2, v));
						}
					}
				}
				else if (count4 == 5 || count4 == 6)
				{
					int num3 = 0;
					int nextOrderedVertex2 = tris[adjacentTris[num3]].GetNextOrderedVertex(l);
					int num4 = num3;
					int currentTriangleVertex = nextOrderedVertex2;
					generatedTileVerts.Clear();
					for (int num5 = 0; num5 < count4; num5++)
					{
						int item = count + adjacentTris[num4];
						generatedTileVerts.Add(item);
						int nextAdjacentTriangle = GetNextAdjacentTriangle(num4, currentTriangleVertex, adjacentTris);
						int nextOrderedVertex3 = tris[adjacentTris[nextAdjacentTriangle]].GetNextOrderedVertex(l);
						num4 = nextAdjacentTriangle;
						currentTriangleVertex = nextOrderedVertex3;
					}
					FinalizeGeneratedTile(generatedTileVerts);
				}
			}
			tris.Clear();
			tris.AddRange(newTris);
		}

		private static void FinalizeGeneratedTile(List<int> generatedTileVerts)
		{
			if ((generatedTileVerts.Count != 5 && generatedTileVerts.Count != 6) || generatedTileVerts.Count > 6)
			{
				Log.Error("Planet shape generation internal error: generated a tile with " + generatedTileVerts.Count + " vertices. Only 5 and 6 are allowed.");
			}
			else if (!ShouldDiscardGeneratedTile(generatedTileVerts))
			{
				int count = tileIDToFinalVerts_offsets.Count;
				tileIDToFinalVerts_offsets.Add(finalVerts.Count);
				int i = 0;
				for (int count2 = generatedTileVerts.Count; i < count2; i++)
				{
					int index = generatedTileVerts[i];
					finalVerts.Add(verts[index]);
					vertToTileIDs_values[vertToTileIDs_values.IndexOf(-1, vertToTileIDs_offsets[index])] = count;
				}
				PackedListOfLists.AddList(tileIDToVerts_offsets, tileIDToVerts_values, generatedTileVerts);
			}
		}

		private static bool ShouldDiscardGeneratedTile(List<int> generatedTileVerts)
		{
			Vector3 zero = Vector3.zero;
			int i = 0;
			for (int count = generatedTileVerts.Count; i < count; i++)
			{
				zero += verts[generatedTileVerts[i]];
			}
			return !MeshUtility.VisibleForWorldgen(zero / generatedTileVerts.Count, radius, viewCenter, viewAngle);
		}

		private static void CalculateTileNeighbors()
		{
			List<int> list = new List<int>();
			int i = 0;
			for (int count = tileIDToVerts_offsets.Count; i < count; i++)
			{
				tmpNeighborsToAdd.Clear();
				PackedListOfLists.GetList(tileIDToVerts_offsets, tileIDToVerts_values, i, tmpVerts);
				int j = 0;
				for (int count2 = tmpVerts.Count; j < count2; j++)
				{
					PackedListOfLists.GetList(vertToTileIDs_offsets, vertToTileIDs_values, tmpVerts[j], tmpTileIDs);
					PackedListOfLists.GetList(vertToTileIDs_offsets, vertToTileIDs_values, tmpVerts[(j + 1) % tmpVerts.Count], list);
					int k = 0;
					for (int count3 = tmpTileIDs.Count; k < count3; k++)
					{
						int num = tmpTileIDs[k];
						if (num != i && num != -1 && list.Contains(num))
						{
							tmpNeighborsToAdd.Add(num);
						}
					}
				}
				PackedListOfLists.AddList(tileIDToNeighbors_offsets, tileIDToNeighbors_values, tmpNeighborsToAdd);
			}
		}

		private static int GetNextAdjacentTriangle(int currentAdjTriangleIndex, int currentTriangleVertex, List<int> adjacentTris)
		{
			int i = 0;
			for (int count = adjacentTris.Count; i < count; i++)
			{
				if (currentAdjTriangleIndex != i)
				{
					TriangleIndices triangleIndices = tris[adjacentTris[i]];
					if (triangleIndices.v1 == currentTriangleVertex || triangleIndices.v2 == currentTriangleVertex || triangleIndices.v3 == currentTriangleVertex)
					{
						return i;
					}
				}
			}
			Log.Error("Planet shape generation internal error: could not find next adjacent triangle.");
			return -1;
		}
	}
}
