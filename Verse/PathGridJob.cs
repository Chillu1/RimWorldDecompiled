using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Verse;

[BurstCompile(CompileSynchronously = true)]
public struct PathGridJob : IJobParallelFor
{
	public const int Cost_OutsideAllowedArea = 600;

	public const int Cost_AvoidFog = 600;

	[ReadOnly]
	public PathFinderCostTuning tuning;

	[ReadOnly]
	public PathFinder.UnmanagedGridTraverseParams traverseParams;

	[ReadOnly]
	public CellIndices indicies;

	[ReadOnly]
	public NativeArray<int>.ReadOnly pathGridDirect;

	[ReadOnly]
	public NativeBitArray.ReadOnly allowedGrid;

	[ReadOnly]
	public NativeBitArray.ReadOnly building;

	[ReadOnly]
	public NativeBitArray.ReadOnly buildingDestroyable;

	[ReadOnly]
	public NativeBitArray.ReadOnly fence;

	[ReadOnly]
	public NativeBitArray.ReadOnly lordGrid;

	[ReadOnly]
	public NativeBitArray.ReadOnly water;

	[ReadOnly]
	public NativeBitArray.ReadOnly darknessDanger;

	[ReadOnly]
	public NativeBitArray.ReadOnly persistentDanger;

	[ReadOnly]
	public NativeBitArray.ReadOnly fogged;

	[ReadOnly]
	public NativeBitArray.ReadOnly player;

	[ReadOnly]
	public NativeArray<byte>.ReadOnly avoidGrid;

	[ReadOnly]
	public NativeArray<ushort>.ReadOnly factionCosts;

	[ReadOnly]
	public NativeArray<ushort>.ReadOnly buildingHitPoints;

	[ReadOnly]
	public NativeArray<ushort>.ReadOnly perceptualCost;

	[ReadOnly]
	public NativeArray<ushort>.ReadOnly custom;

	public NativeArray<int> grid;

	private bool passAllDestroyableThings;

	private bool passWater;

	[BurstCompile]
	public void Execute(int index)
	{
		passAllDestroyableThings = traverseParams.mode == TraverseMode.PassAllDestroyableThings || traverseParams.mode == TraverseMode.PassAllDestroyableThingsNotWater;
		passWater = traverseParams.mode != TraverseMode.NoPassClosedDoorsOrWater && traverseParams.mode != TraverseMode.PassAllDestroyableThingsNotWater;
		if (!CellIsPassable(index))
		{
			grid[index] = 10000;
		}
		else
		{
			grid[index] = CostForCell(index);
		}
	}

	private bool DestroyableBuilding(int index)
	{
		if (pathGridDirect[index] < 10000)
		{
			return false;
		}
		if (traverseParams.fenceBlocked && traverseParams.canBashFences && fence.IsSet(index))
		{
			return true;
		}
		if (!building.IsSet(index))
		{
			return false;
		}
		if (!buildingDestroyable.IsSet(index))
		{
			return false;
		}
		if (traverseParams.mode == TraverseMode.PassAllDestroyablePlayerOwnedThings && player.IsSet(index))
		{
			return true;
		}
		return passAllDestroyableThings;
	}

	private bool CellIsPassable(int index)
	{
		if (pathGridDirect[index] >= 10000 && !DestroyableBuilding(index))
		{
			return false;
		}
		if (custom.Length > 0 && custom[index] >= 10000)
		{
			return false;
		}
		if (!passWater && water.IsSet(index))
		{
			return false;
		}
		if (traverseParams.fenceBlocked && !traverseParams.canBashFences && fence.IsSet(index))
		{
			return false;
		}
		return true;
	}

	private int CostForCell(int index)
	{
		int num = (DestroyableBuilding(index) ? (tuning.costBlockedWallBase + Mathf.RoundToInt((float)(int)buildingHitPoints[index] * tuning.costBlockedWallExtraPerHitPoint)) : ((tuning.costWater < 0 || !water.IsSet(index)) ? pathGridDirect[index] : tuning.costWater));
		if (tuning.costWater <= 0 || !water.IsSet(index))
		{
			num += perceptualCost[index];
		}
		if (avoidGrid.Length > 0)
		{
			num += avoidGrid[index] * 8;
		}
		if (allowedGrid.Length > 0 && !allowedGrid.IsSet(index))
		{
			num += 600;
		}
		if (traverseParams.fenceBlocked && fence.IsSet(index) && passAllDestroyableThings)
		{
			num += tuning.costBlockedDoor + Mathf.RoundToInt((float)(int)buildingHitPoints[index] * tuning.costBlockedDoorPerHitPoint);
		}
		IntVec3 c = indicies[index];
		if (traverseParams.targetBuildable.Area == 0 || !traverseParams.targetBuildable.Contains(c))
		{
			num += factionCosts[index];
		}
		if (traverseParams.avoidPersistentDanger && persistentDanger.IsSet(index))
		{
			num += tuning.costDanger;
		}
		else if (traverseParams.avoidDarknessDanger && darknessDanger.IsSet(index))
		{
			num += tuning.costDanger;
		}
		if (lordGrid.Length > 0 && !lordGrid.IsSet(index))
		{
			num += tuning.costOffLordWalkGrid;
		}
		if (traverseParams.avoidFog && fogged.IsSet(index))
		{
			num += 600;
		}
		if (custom.Length > 0)
		{
			num += custom[index];
		}
		return num;
	}
}
