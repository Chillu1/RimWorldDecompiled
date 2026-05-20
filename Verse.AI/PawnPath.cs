using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Verse.AI;

public class PawnPath : IDisposable
{
	private readonly List<IntVec3> nodes = new List<IntVec3>(128);

	private float totalCostInt;

	private int curNodeIndex;

	private bool usedRegionHeuristics;

	private bool inUse;

	internal PawnPathPool pool;

	private static readonly List<IntVec3> tmpPeekCells = new List<IntVec3>();

	public bool Found => totalCostInt >= 0f;

	public bool Finished => NodesLeftCount <= 0;

	public float TotalCost => totalCostInt;

	public int NodesLeftCount => curNodeIndex + 1;

	public int NodesConsumedCount => nodes.Count - NodesLeftCount;

	public bool UsedRegionHeuristics => usedRegionHeuristics;

	public List<IntVec3> NodesReversed => nodes;

	public IntVec3 FirstNode
	{
		get
		{
			List<IntVec3> list = nodes;
			return list[list.Count - 1];
		}
	}

	public IntVec3 LastNode => nodes[0];

	public static PawnPath NotFound => new PawnPath
	{
		totalCostInt = -1f
	};

	internal void Initialize(NativeList<IntVec3> points, int cost)
	{
		if (!Found)
		{
			Log.Warning("Calling initialize on invalid path");
			return;
		}
		inUse = true;
		for (int i = 0; i < points.Length; i++)
		{
			nodes.Add(points[i]);
		}
		curNodeIndex = nodes.Count - 1;
		totalCostInt = cost;
	}

	public void AddNode(IntVec3 nodePosition)
	{
		nodes.Add(nodePosition);
	}

	public void Dispose()
	{
		ReleaseToPool();
	}

	public void ReleaseToPool()
	{
		if (Found)
		{
			totalCostInt = 0f;
			usedRegionHeuristics = false;
			nodes.Clear();
			inUse = false;
			if (pool != null)
			{
				pool.ReturnPath(this);
			}
		}
	}

	public IntVec3 ConsumeNextNode()
	{
		IntVec3 result = Peek(1);
		curNodeIndex--;
		return result;
	}

	public bool BacktrackNode()
	{
		if (curNodeIndex >= nodes.Count - 1)
		{
			return false;
		}
		curNodeIndex++;
		return true;
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
		return "PawnPath(nodeCount= " + nodes.Count + ((nodes.Count > 0) ? $" first={FirstNode} last={LastNode}" : "") + " cost=" + totalCostInt + " )";
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
			Vector3 vector = Peek(0).ToVector3Shifted();
			vector.y = y;
			if ((drawPos - vector).sqrMagnitude > 0.01f)
			{
				GenDraw.DrawLineBetween(drawPos, vector);
			}
		}
	}

	public List<IntVec3> PeekNextCells(int count, int curWaypointIndexOffset = 0)
	{
		PeekNextCells(count, tmpPeekCells, curWaypointIndexOffset);
		return tmpPeekCells;
	}

	public void PeekNextCells(int count, List<IntVec3> outCells, int offset = 1)
	{
		outCells.Clear();
		if (!Finished)
		{
			int num = Math.Max(curNodeIndex - offset - count, 0);
			for (int num2 = curNodeIndex - offset; num2 >= num; num2--)
			{
				outCells.Add(nodes[num2]);
			}
		}
	}
}
