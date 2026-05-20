using System;
using RimWorld;

namespace Verse;

public struct PathFinderCostTuning : IEquatable<PathFinderCostTuning>
{
	private const int Cost_BlockedWallBase = 70;

	private const float Cost_BlockedWallExtraPerHitPoint = 0.2f;

	private const int Cost_BlockedWallExtraForNaturalWalls = 0;

	private const int Cost_BlockedDoor = 50;

	private const float Cost_BlockedDoorPerHitPoint = 0.2f;

	private const int Cost_OffLordWalkGrid = 70;

	private const int Cost_Danger = 300;

	private const int Cost_BlueprintFrame = 800;

	public int costBlockedWallBase;

	public float costBlockedWallExtraPerHitPoint;

	public int costBlockedWallExtraForNaturalWalls;

	public int costBlockedDoor;

	public float costBlockedDoorPerHitPoint;

	public int costOffLordWalkGrid;

	public int costDanger;

	public int costWater;

	public int costBlueprintFrame;

	public static readonly PathFinderCostTuning DefaultTuning = new PathFinderCostTuning(70, 0.2f, 0, 50, 0.2f, 70, 300, 800);

	public static PathFinderCostTuning For(Pawn pawn)
	{
		PathFinderCostTuning defaultTuning = DefaultTuning;
		if (pawn.WaterCellCost.HasValue)
		{
			defaultTuning.costWater = pawn.WaterCellCost.Value;
		}
		if (!pawn.RaceProps.ToolUser || pawn.HostileTo(Faction.OfPlayer))
		{
			defaultTuning.costBlueprintFrame = 0;
		}
		return defaultTuning;
	}

	public PathFinderCostTuning(int costBlockedWallBase, float costBlockedWallExtraPerHitPoint, int costBlockedWallExtraForNaturalWalls, int costBlockedDoor, float costBlockedDoorPerHitPoint, int costOffLordWalkGrid, int costDanger, int costBlueprintFrame, int costWater = -1)
	{
		this.costBlockedWallBase = costBlockedWallBase;
		this.costBlockedWallExtraPerHitPoint = costBlockedWallExtraPerHitPoint;
		this.costBlockedWallExtraForNaturalWalls = costBlockedWallExtraForNaturalWalls;
		this.costBlockedDoor = costBlockedDoor;
		this.costBlockedDoorPerHitPoint = costBlockedDoorPerHitPoint;
		this.costOffLordWalkGrid = costOffLordWalkGrid;
		this.costDanger = costDanger;
		this.costBlueprintFrame = costBlueprintFrame;
		this.costWater = costWater;
	}

	public bool Equals(PathFinderCostTuning other)
	{
		if (costBlockedWallBase == other.costBlockedWallBase && costBlockedWallExtraPerHitPoint.Equals(other.costBlockedWallExtraPerHitPoint) && costBlockedWallExtraForNaturalWalls == other.costBlockedWallExtraForNaturalWalls && costBlockedDoor == other.costBlockedDoor && costBlockedDoorPerHitPoint.Equals(other.costBlockedDoorPerHitPoint) && costOffLordWalkGrid == other.costOffLordWalkGrid && costDanger == other.costDanger && costBlueprintFrame == other.costBlueprintFrame)
		{
			return costWater == other.costWater;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PathFinderCostTuning other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(HashCode.Combine(costBlockedWallBase, costBlockedWallExtraPerHitPoint, costBlockedWallExtraForNaturalWalls, costBlockedDoor, costBlockedDoorPerHitPoint, costOffLordWalkGrid, costDanger, costBlueprintFrame), costWater);
	}
}
