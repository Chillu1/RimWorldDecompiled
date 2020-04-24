using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPath : IDisposable
	{
		private List<int> nodes = new List<int>(128);

		private float totalCostInt;

		private int curNodeIndex;

		public bool inUse;

		public bool Found => totalCostInt >= 0f;

		public float TotalCost => totalCostInt;

		public int NodesLeftCount => curNodeIndex + 1;

		public List<int> NodesReversed => nodes;

		public int FirstNode => nodes[nodes.Count - 1];

		public int LastNode => nodes[0];

		public static WorldPath NotFound => WorldPathPool.NotFoundPath;

		public void AddNodeAtStart(int tile)
		{
			nodes.Add(tile);
		}

		public void SetupFound(float totalCost)
		{
			if (this == NotFound)
			{
				Log.Warning("Calling SetupFound with totalCost=" + totalCost + " on WorldPath.NotFound");
				return;
			}
			totalCostInt = totalCost;
			curNodeIndex = nodes.Count - 1;
		}

		public void Dispose()
		{
			ReleaseToPool();
		}

		public void ReleaseToPool()
		{
			if (this != NotFound)
			{
				totalCostInt = 0f;
				nodes.Clear();
				inUse = false;
			}
		}

		public static WorldPath NewNotFound()
		{
			return new WorldPath
			{
				totalCostInt = -1f
			};
		}

		public int ConsumeNextNode()
		{
			int result = Peek(1);
			curNodeIndex--;
			return result;
		}

		public int Peek(int nodesAhead)
		{
			return nodes[curNodeIndex - nodesAhead];
		}

		public override string ToString()
		{
			if (!Found)
			{
				return "WorldPath(not found)";
			}
			if (!inUse)
			{
				return "WorldPath(not in use)";
			}
			return "WorldPath(nodeCount= " + nodes.Count + ((nodes.Count > 0) ? (" first=" + FirstNode + " last=" + LastNode) : "") + " cost=" + totalCostInt + " )";
		}

		public void DrawPath(Caravan pathingCaravan)
		{
			if (!Found || NodesLeftCount <= 0)
			{
				return;
			}
			WorldGrid worldGrid = Find.WorldGrid;
			float d = 0.05f;
			for (int i = 0; i < NodesLeftCount - 1; i++)
			{
				Vector3 tileCenter = worldGrid.GetTileCenter(Peek(i));
				Vector3 tileCenter2 = worldGrid.GetTileCenter(Peek(i + 1));
				tileCenter += tileCenter.normalized * d;
				tileCenter2 += tileCenter2.normalized * d;
				GenDraw.DrawWorldLineBetween(tileCenter, tileCenter2);
			}
			if (pathingCaravan != null)
			{
				Vector3 drawPos = pathingCaravan.DrawPos;
				Vector3 tileCenter3 = worldGrid.GetTileCenter(Peek(0));
				drawPos += drawPos.normalized * d;
				tileCenter3 += tileCenter3.normalized * d;
				if ((drawPos - tileCenter3).sqrMagnitude > 0.005f)
				{
					GenDraw.DrawWorldLineBetween(drawPos, tileCenter3);
				}
			}
		}
	}
}
