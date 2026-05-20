using LudeonTK;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Verse;

[BurstCompile(CompileSynchronously = true)]
public struct PathFinderJob : IJob
{
	public struct CalcNode
	{
		public enum Status : byte
		{
			None,
			Open,
			Closed
		}

		public int gCost;

		public float hCost;

		public float fCost;

		public int parentIndex;

		public Status status;
	}

	public struct ResultData
	{
		public int pathCost;
	}

	private const int Cost_Blocked = 175;

	public CellIndices indices;

	public float heuristicStrength;

	public int moveTicksCardinal;

	public int moveTicksDiagonal;

	public IntVec3 start;

	public IntVec3 destCell;

	public CellRect destRect;

	public PathFinder.UnmanagedGridTraverseParams traverseParams;

	public NativeArray<CalcNode> calcGrid;

	public NativePriorityQueue<int, float, FloatMinComparer> frontier;

	public NativeList<IntVec3> path;

	public NativeReference<ResultData> result;

	[ReadOnly]
	public NativeArray<int>.ReadOnly grid;

	[ReadOnly]
	public NativeArray<ushort>.ReadOnly providerCost;

	[ReadOnly]
	public NativeArray<bool>.ReadOnly blocked;

	[ReadOnly]
	public NativeList<int> excludedRectIndices;

	[ReadOnly]
	public NativeArray<CellConnection>.ReadOnly connectivity;

	[ReadOnly]
	public NativeBitArray.ReadOnly fences;

	[ReadOnly]
	public NativeBitArray.ReadOnly buildings;

	private bool passAllDestroyableThings;

	private bool passWater;

	[BurstCompile]
	public void Execute()
	{
		calcGrid.Clear();
		frontier.Clear();
		path.Clear();
		result.Value = default(ResultData);
		int num = indices.CellToIndex(destCell);
		bool flag = destRect.Area == 1;
		int num2 = 0;
		InitalizeSearch();
		while (frontier.Count > 0)
		{
			frontier.Dequeue(out var element, out var priority);
			if (!Mathf.Approximately(priority, calcGrid[element].fCost) || calcGrid[element].status == CalcNode.Status.Closed)
			{
				continue;
			}
			IntVec3 intVec = indices.IndexToCell(element);
			if (flag)
			{
				if (element == num)
				{
					FinalizePath(element);
					break;
				}
			}
			else if (destRect.Contains(intVec) && !excludedRectIndices.Contains(element))
			{
				FinalizePath(element);
				break;
			}
			if (num2 >= indices.NumGridCells)
			{
				break;
			}
			CellConnection connections = connectivity[element];
			int num3 = connections.BitLoopEndIndex();
			for (int i = 0; i < num3; i++)
			{
				CellConnection cellConnection = CellConnectionExtensions.FlagFromBitIndex[i];
				if (!connections.HasBit(cellConnection))
				{
					continue;
				}
				IntVec3 intVec2 = intVec + CellConnectionExtensions.OffsetFromBitIndex[i];
				int num4 = indices.CellToIndex(intVec2);
				if (calcGrid[num4].status == CalcNode.Status.Closed)
				{
					continue;
				}
				int num5 = IndexCost(num4);
				if (num5 >= 10000)
				{
					continue;
				}
				if (cellConnection.Diagonal())
				{
					int num6 = indices.CellToIndex(new IntVec3(intVec2.x, 0, intVec.z));
					int num7 = indices.CellToIndex(new IntVec3(intVec.x, 0, intVec2.z));
					if (IndexCost(num6) >= 10000 || IndexCost(num7) >= 10000 || buildings.IsSet(num6) || (!traverseParams.canBashFences && fences.IsSet(num6)) || buildings.IsSet(num7) || (!traverseParams.canBashFences && fences.IsSet(num7)))
					{
						continue;
					}
				}
				num5 = ((!cellConnection.Diagonal()) ? (num5 + moveTicksCardinal) : (num5 + moveTicksDiagonal));
				int num8 = num5 + calcGrid[element].gCost;
				CalcNode.Status status = calcGrid[num4].status;
				if (status != CalcNode.Status.None)
				{
					int num9 = 0;
					if (status == CalcNode.Status.Closed)
					{
						num9 = Mathf.CeilToInt((float)moveTicksCardinal * 0.8f);
					}
					if (calcGrid[num4].gCost <= num8 + num9)
					{
						continue;
					}
				}
				if (status == CalcNode.Status.None)
				{
					IntVec3 intVec3 = destCell - intVec2;
					if (intVec3.x != 0 || intVec3.z != 0)
					{
						int num10 = math.abs(intVec3.x);
						int num11 = math.abs(intVec3.z);
						float num12 = 1f * (float)(num10 + num11) + -0.58579004f * (float)math.min(num10, num11);
						CalcNode value = calcGrid[num4];
						value.hCost = num12 * 13f * heuristicStrength;
						calcGrid[num4] = value;
					}
				}
				float num13 = (float)num8 + calcGrid[num4].hCost;
				if (num13 < 0f)
				{
					num13 = 0f;
				}
				CalcNode value2 = calcGrid[num4];
				value2.gCost = num8;
				value2.fCost = num13;
				value2.parentIndex = element;
				value2.status = CalcNode.Status.Open;
				calcGrid[num4] = value2;
				frontier.Enqueue(num4, num13);
			}
			num2++;
			CalcNode value3 = calcGrid[element];
			value3.status = CalcNode.Status.Closed;
			calcGrid[element] = value3;
		}
	}

	private int IndexCost(int index)
	{
		if (providerCost[index] == ushort.MaxValue)
		{
			return 10000;
		}
		return math.max(grid[index], providerCost[index]) + (blocked[index] ? 175 : 0);
	}

	private void InitalizeSearch()
	{
		int num = indices.CellToIndex(start);
		CalcNode value = calcGrid[num];
		value.parentIndex = num;
		value.status = CalcNode.Status.Open;
		calcGrid[num] = value;
		frontier.Enqueue(num, 0f);
	}

	private void FinalizePath(int finalIndex)
	{
		int num = finalIndex;
		while (true)
		{
			path.Add(indices.IndexToCell(num));
			int parentIndex = calcGrid[num].parentIndex;
			if (num == parentIndex)
			{
				break;
			}
			num = parentIndex;
		}
		ResultData value = default(ResultData);
		value.pathCost = calcGrid[finalIndex].gCost;
		result.Value = value;
	}
}
