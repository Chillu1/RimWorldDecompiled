using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WorldLayer_Paths : WorldLayer
	{
		public struct OutputDirection
		{
			public int neighbor;

			public float width;

			public float distortionFrequency;

			public float distortionIntensity;
		}

		protected bool pointyEnds;

		private List<Vector3> tmpVerts = new List<Vector3>();

		private List<Vector3> tmpHexVerts = new List<Vector3>();

		private List<int> tmpNeighbors = new List<int>();

		private static List<int> lhsID = new List<int>();

		private static List<int> rhsID = new List<int>();

		public void GeneratePaths(LayerSubMesh subMesh, int tileID, List<OutputDirection> nodes, Color32 color, bool allowSmoothTransition)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			worldGrid.GetTileVertices(tileID, tmpVerts);
			worldGrid.GetTileNeighbors(tileID, tmpNeighbors);
			if (nodes.Count == 1 && pointyEnds)
			{
				int count = subMesh.verts.Count;
				AddPathEndpoint(subMesh, tmpVerts, tmpNeighbors.IndexOf(nodes[0].neighbor), color, tileID, nodes[0]);
				subMesh.verts.Add(FinalizePoint(worldGrid.GetTileCenter(tileID), nodes[0].distortionFrequency, nodes[0].distortionIntensity));
				subMesh.colors.Add(color.MutateAlpha(0));
				subMesh.tris.Add(count);
				subMesh.tris.Add(count + 3);
				subMesh.tris.Add(count + 1);
				subMesh.tris.Add(count + 1);
				subMesh.tris.Add(count + 3);
				subMesh.tris.Add(count + 2);
				return;
			}
			if (nodes.Count == 2)
			{
				int count2 = subMesh.verts.Count;
				int num = tmpNeighbors.IndexOf(nodes[0].neighbor);
				int num2 = tmpNeighbors.IndexOf(nodes[1].neighbor);
				if (allowSmoothTransition && Mathf.Abs(num - num2) > 1 && Mathf.Abs((num - num2 + tmpVerts.Count) % tmpVerts.Count) > 1)
				{
					AddPathEndpoint(subMesh, tmpVerts, num, color, tileID, nodes[0]);
					AddPathEndpoint(subMesh, tmpVerts, num2, color, tileID, nodes[1]);
					subMesh.tris.Add(count2);
					subMesh.tris.Add(count2 + 5);
					subMesh.tris.Add(count2 + 1);
					subMesh.tris.Add(count2 + 5);
					subMesh.tris.Add(count2 + 4);
					subMesh.tris.Add(count2 + 1);
					subMesh.tris.Add(count2 + 1);
					subMesh.tris.Add(count2 + 4);
					subMesh.tris.Add(count2 + 2);
					subMesh.tris.Add(count2 + 4);
					subMesh.tris.Add(count2 + 3);
					subMesh.tris.Add(count2 + 2);
					return;
				}
			}
			float num3 = 0f;
			for (int i = 0; i < nodes.Count; i++)
			{
				num3 = Mathf.Max(num3, nodes[i].width);
			}
			Vector3 tileCenter = worldGrid.GetTileCenter(tileID);
			tmpHexVerts.Clear();
			for (int j = 0; j < tmpVerts.Count; j++)
			{
				tmpHexVerts.Add(FinalizePoint(Vector3.LerpUnclamped(tileCenter, tmpVerts[j], num3 * 0.5f * 2f), 0f, 0f));
			}
			tileCenter = FinalizePoint(tileCenter, 0f, 0f);
			int count3 = subMesh.verts.Count;
			subMesh.verts.Add(tileCenter);
			subMesh.colors.Add(color);
			int count4 = subMesh.verts.Count;
			for (int k = 0; k < tmpHexVerts.Count; k++)
			{
				subMesh.verts.Add(tmpHexVerts[k]);
				subMesh.colors.Add(color.MutateAlpha(0));
				subMesh.tris.Add(count3);
				subMesh.tris.Add(count4 + (k + 1) % tmpHexVerts.Count);
				subMesh.tris.Add(count4 + k);
			}
			for (int l = 0; l < nodes.Count; l++)
			{
				if (nodes[l].width != 0f)
				{
					int count5 = subMesh.verts.Count;
					int num4 = tmpNeighbors.IndexOf(nodes[l].neighbor);
					AddPathEndpoint(subMesh, tmpVerts, num4, color, tileID, nodes[l]);
					subMesh.tris.Add(count5);
					subMesh.tris.Add(count4 + (num4 + tmpHexVerts.Count - 1) % tmpHexVerts.Count);
					subMesh.tris.Add(count3);
					subMesh.tris.Add(count5);
					subMesh.tris.Add(count3);
					subMesh.tris.Add(count5 + 1);
					subMesh.tris.Add(count5 + 1);
					subMesh.tris.Add(count3);
					subMesh.tris.Add(count5 + 2);
					subMesh.tris.Add(count3);
					subMesh.tris.Add(count4 + (num4 + 2) % tmpHexVerts.Count);
					subMesh.tris.Add(count5 + 2);
				}
			}
		}

		private void AddPathEndpoint(LayerSubMesh subMesh, List<Vector3> verts, int index, Color32 color, int tileID, OutputDirection data)
		{
			int index2 = (index + 1) % verts.Count;
			Find.WorldGrid.GetTileNeighbors(tileID, lhsID);
			Find.WorldGrid.GetTileNeighbors(data.neighbor, rhsID);
			float num = lhsID.Intersect(rhsID).Any((int id) => Find.WorldGrid[id].WaterCovered) ? 0.5f : 1f;
			Vector3 a = FinalizePoint(verts[index], data.distortionFrequency, data.distortionIntensity * num);
			Vector3 b = FinalizePoint(verts[index2], data.distortionFrequency, data.distortionIntensity * num);
			subMesh.verts.Add(Vector3.LerpUnclamped(a, b, 0.5f - data.width));
			subMesh.colors.Add(color.MutateAlpha(0));
			subMesh.verts.Add(Vector3.LerpUnclamped(a, b, 0.5f));
			subMesh.colors.Add(color);
			subMesh.verts.Add(Vector3.LerpUnclamped(a, b, 0.5f + data.width));
			subMesh.colors.Add(color.MutateAlpha(0));
		}

		public abstract Vector3 FinalizePoint(Vector3 inp, float distortionFrequency, float distortionIntensity);
	}
}
