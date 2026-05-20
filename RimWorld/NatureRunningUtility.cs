using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class NatureRunningUtility
{
	private const int MinRegionExtantDistanceFromVisited = 15;

	private const int MinRegionExtantDistanceFromBuildingRegion = 15;

	private const int MaxRegionsSearch = 300;

	private const int RegionsToCheckAfterValidFound = 8;

	private static readonly Dictionary<Region, bool> tmpRegionContainsOwnedBuildingCache = new Dictionary<Region, bool>();

	public static bool TryFindNatureInterestTarget(Pawn searcher, out LocalTargetInfo interestTarget)
	{
		interestTarget = LocalTargetInfo.Invalid;
		if (!JoyUtility.EnjoyableOutsideNow(searcher))
		{
			return false;
		}
		tmpRegionContainsOwnedBuildingCache.Clear();
		LocalTargetInfo bfsTarget = LocalTargetInfo.Invalid;
		TraverseParms traverseParms = TraverseParms.For(searcher);
		int additionalRegionsToCheck = 8;
		RegionTraverser.BreadthFirstTraverse(searcher.Position, searcher.Map, (Region from, Region r) => r.Allows(traverseParms, isDestination: false), delegate(Region r)
		{
			if (bfsTarget.IsValid && --additionalRegionsToCheck <= 0)
			{
				return true;
			}
			if (r.IsForbiddenEntirely(searcher))
			{
				return false;
			}
			if (r.DangerFor(searcher) == Danger.Deadly)
			{
				return false;
			}
			if (r.extentsClose.ClosestDistSquaredTo(searcher.Position) < 225f)
			{
				return false;
			}
			CellRect checkNearbyRegionsTouchingArea = r.extentsClose.ExpandedBy(15);
			bool foundNearbyRegionWithBuildings = false;
			RegionTraverser.BreadthFirstTraverse(r, (Region from, Region to) => to.extentsClose.Overlaps(checkNearbyRegionsTouchingArea), delegate(Region region)
			{
				if (tmpRegionContainsOwnedBuildingCache.TryGetValue(region, out var value))
				{
					if (value)
					{
						foundNearbyRegionWithBuildings = true;
						return true;
					}
					return false;
				}
				foreach (Thing item in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
				{
					if (item.Faction != null)
					{
						foundNearbyRegionWithBuildings = true;
						tmpRegionContainsOwnedBuildingCache[region] = true;
						return true;
					}
				}
				tmpRegionContainsOwnedBuildingCache[region] = false;
				return false;
			});
			if (foundNearbyRegionWithBuildings)
			{
				return false;
			}
			if (r.TryFindRandomCellInRegion(CellValidator, out var result) && (!bfsTarget.IsValid || searcher.Position.DistanceTo(bfsTarget.Cell) > searcher.Position.DistanceTo(result)))
			{
				bfsTarget = result;
			}
			return false;
		}, 300);
		tmpRegionContainsOwnedBuildingCache.Clear();
		interestTarget = bfsTarget;
		return interestTarget.IsValid;
		bool CellValidator(IntVec3 c)
		{
			if (c.IsForbidden(searcher))
			{
				return false;
			}
			TerrainDef terrain = c.GetTerrain(searcher.Map);
			if (!terrain.avoidWander)
			{
				return terrain.natural;
			}
			return false;
		}
	}
}
