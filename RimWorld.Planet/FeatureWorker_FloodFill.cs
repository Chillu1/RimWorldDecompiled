using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public abstract class FeatureWorker_FloodFill : FeatureWorker
{
	private readonly List<PlanetTile> roots = new List<PlanetTile>();

	private readonly HashSet<PlanetTile> rootsSet = new HashSet<PlanetTile>();

	private readonly List<PlanetTile> possiblyAllowed = new List<PlanetTile>();

	private readonly HashSet<PlanetTile> possiblyAllowedSet = new HashSet<PlanetTile>();

	private readonly List<PlanetTile> currentGroup = new List<PlanetTile>();

	private readonly List<PlanetTile> currentGroupMembers = new List<PlanetTile>();

	private static readonly List<PlanetTile> TmpGroup = new List<PlanetTile>();

	protected virtual int MinSize => def.minSize;

	protected virtual int MaxSize => def.maxSize;

	protected virtual int MaxPossiblyAllowedSizeToTake => def.maxPossiblyAllowedSizeToTake;

	protected virtual float MaxPossiblyAllowedSizePctOfMeToTake => def.maxPossiblyAllowedSizePctOfMeToTake;

	protected abstract bool IsRoot(PlanetTile tile);

	protected virtual bool IsPossiblyAllowed(PlanetTile tile)
	{
		return false;
	}

	protected virtual bool IsMember(PlanetTile tile)
	{
		return Find.WorldGrid[tile].feature == null;
	}

	public override void GenerateWhereAppropriate(PlanetLayer layer)
	{
		CalculateRootsAndPossiblyAllowedTiles(layer);
		CalculateContiguousGroups(layer);
	}

	private void CalculateRootsAndPossiblyAllowedTiles(PlanetLayer layer)
	{
		roots.Clear();
		possiblyAllowed.Clear();
		int tilesCount = layer.TilesCount;
		for (int i = 0; i < tilesCount; i++)
		{
			PlanetTile tile = layer[i].tile;
			if (IsRoot(tile))
			{
				roots.Add(tile);
			}
			if (IsPossiblyAllowed(tile))
			{
				possiblyAllowed.Add(tile);
			}
		}
		rootsSet.Clear();
		rootsSet.AddRange(roots);
		possiblyAllowedSet.Clear();
		possiblyAllowedSet.AddRange(possiblyAllowed);
	}

	private void CalculateContiguousGroups(PlanetLayer layer)
	{
		WorldFloodFiller filler = layer.Filler;
		int minSize = MinSize;
		int maxSize = MaxSize;
		int maxPossiblyAllowedSizeToTake = MaxPossiblyAllowedSizeToTake;
		float maxPossiblyAllowedSizePctOfMeToTake = MaxPossiblyAllowedSizePctOfMeToTake;
		FeatureWorker.ClearVisited(layer);
		FeatureWorker.ClearGroupSizes(layer);
		for (int i = 0; i < possiblyAllowed.Count; i++)
		{
			PlanetTile planetTile = possiblyAllowed[i];
			if (!FeatureWorker.visited[planetTile.tileId] && !rootsSet.Contains(planetTile))
			{
				TmpGroup.Clear();
				filler.FloodFill(planetTile, (PlanetTile x) => possiblyAllowedSet.Contains(x) && !rootsSet.Contains(x), delegate(PlanetTile x)
				{
					FeatureWorker.visited[x.tileId] = true;
					TmpGroup.Add(x);
				});
				for (int num = 0; num < TmpGroup.Count; num++)
				{
					FeatureWorker.groupSize[TmpGroup[num].tileId] = TmpGroup.Count;
				}
			}
		}
		for (int num2 = 0; num2 < roots.Count; num2++)
		{
			PlanetTile rootTile = roots[num2];
			if (FeatureWorker.visited[rootTile.tileId])
			{
				continue;
			}
			int initialMembersCountClamped = 0;
			filler.FloodFill(rootTile, (PlanetTile x) => (rootsSet.Contains(x) || possiblyAllowedSet.Contains(x)) && IsMember(x), delegate(PlanetTile x)
			{
				FeatureWorker.visited[x.tileId] = true;
				initialMembersCountClamped++;
				return initialMembersCountClamped >= minSize;
			});
			if (initialMembersCountClamped < minSize)
			{
				continue;
			}
			int initialRootsCount = 0;
			filler.FloodFill(rootTile, (PlanetTile x) => rootsSet.Contains(x), delegate(PlanetTile x)
			{
				FeatureWorker.visited[x.tileId] = true;
				initialRootsCount++;
			});
			if (initialRootsCount < minSize || initialRootsCount > maxSize)
			{
				continue;
			}
			int traversedRootsCount = 0;
			currentGroup.Clear();
			filler.FloodFill(rootTile, (PlanetTile x) => rootsSet.Contains(x) || (possiblyAllowedSet.Contains(x) && FeatureWorker.groupSize[x.tileId] <= maxPossiblyAllowedSizeToTake && (float)FeatureWorker.groupSize[x.tileId] <= maxPossiblyAllowedSizePctOfMeToTake * (float)Mathf.Max(traversedRootsCount, initialRootsCount) && FeatureWorker.groupSize[x.tileId] < maxSize), delegate(PlanetTile x)
			{
				FeatureWorker.visited[x.tileId] = true;
				if (rootsSet.Contains(x))
				{
					traversedRootsCount++;
				}
				currentGroup.Add(x);
			});
			if (currentGroup.Count < minSize || currentGroup.Count > maxSize || (!def.canTouchWorldEdge && currentGroup.Any((PlanetTile x) => Find.WorldGrid.IsOnEdge(x))))
			{
				continue;
			}
			currentGroupMembers.Clear();
			for (int num3 = 0; num3 < currentGroup.Count; num3++)
			{
				if (IsMember(currentGroup[num3]))
				{
					currentGroupMembers.Add(currentGroup[num3]);
				}
			}
			if (currentGroupMembers.Count < minSize)
			{
				continue;
			}
			if (currentGroup.Any((PlanetTile x) => layer[x].feature == null))
			{
				currentGroup.RemoveAll((PlanetTile x) => layer[x].feature != null);
			}
			AddFeature(layer, currentGroupMembers, currentGroup);
		}
	}
}
