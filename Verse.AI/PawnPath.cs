using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public class PawnPath : IDisposable
	{
		private List<IntVec3> nodes = new List<IntVec3>(128);

		private float totalCostInt;

		private int curNodeIndex;

		private bool usedRegionHeuristics;

		public bool inUse;

		public bool Found => totalCostInt >= 0f;

		public float TotalCost => totalCostInt;

		public int NodesLeftCount => curNodeIndex + 1;

		public int NodesConsumedCount => nodes.Count - NodesLeftCount;

		public bool UsedRegionHeuristics => usedRegionHeuristics;

		public List<IntVec3> NodesReversed => nodes;

		public IntVec3 FirstNode => nodes[nodes.Count - 1];

		public IntVec3 LastNode => nodes[0];

		public static PawnPath NotFound => PawnPathPool.NotFoundPath;

		public void AddNode(IntVec3 nodePosition)
		{
			nodes.Add(nodePosition);
		}

		public void SetupFound(float totalCost, bool usedRegionHeuristics)
		{
			if (this == NotFound)
			{
				Log.Warning("Calling SetupFound with totalCost=" + totalCost + " on PawnPath.NotFound");
				return;
			}
			totalCostInt = totalCost;
			this.usedRegionHeuristics = usedRegionHeuristics;
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
				usedRegionHeuristics = false;
				nodes.Clear();
				inUse = false;
			}
		}

		public static PawnPath NewNotFound()
		{
			return new PawnPath
			{
				totalCostInt = -1f
			};
		}

		public IntVec3 ConsumeNextNode()
		{
			IntVec3 result = Peek(1);
			curNodeIndex--;
			return result;
		}

		public IntVec3 Peek(int nodesAhead)
		{
			return nodes[curNodeIndex - nodesAhead];
		}

		public override string ToString()
		{
			if (!Found)
			{
				return "PawnPath(not found)";
			}
			if (!inUse)
			{
				return "PawnPath(not in use)";
			}
			return "PawnPath(nodeCount= " + nodes.Count + ((nodes.Count > 0) ? (" first=" + FirstNode + " last=" + LastNode) : "") + " cost=" + totalCostInt + " )";
		}

		public void DrawPath(Pawn pathingPawn)
		{
			if (!Found)
			{
				return;
			}
			float y = AltitudeLayer.Item.AltitudeFor();
			if (NodesLeftCount <= 0)
			{
				return;
			}
			for (int i = 0; i < NodesLeftCount - 1; i++)
			{
				Vector3 a = Peek(i).ToVector3Shifted();
				a.y = y;
				Vector3 b = Peek(i + 1).ToVector3Shifted();
				b.y = y;
				GenDraw.DrawLineBetween(a, b);
			}
			if (pathingPawn != null)
			{
				Vector3 drawPos = pathingPawn.DrawPos;
				drawPos.y = y;
				Vector3 b2 = Peek(0).ToVector3Shifted();
				b2.y = y;
				if ((drawPos - b2).sqrMagnitude > 0.01f)
				{
					GenDraw.DrawLineBetween(drawPos, b2);
				}
			}
		}
	}
}
