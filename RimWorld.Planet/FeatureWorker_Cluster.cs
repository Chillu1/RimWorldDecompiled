using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public abstract class FeatureWorker_Cluster : FeatureWorker
	{
		private List<int> roots = new List<int>();

		private HashSet<int> rootsSet = new HashSet<int>();

		private List<int> rootsWithAreaInBetween = new List<int>();

		private HashSet<int> rootsWithAreaInBetweenSet = new HashSet<int>();

		private List<int> currentGroup = new List<int>();

		private List<int> currentGroupMembers = new List<int>();

		private HashSet<int> visitedValidGroupIDs = new HashSet<int>();

		private static List<int> tmpGroup = new List<int>();

		protected virtual int MinRootGroupsInCluster => def.minRootGroupsInCluster;

		protected virtual int MinRootGroupSize => def.minRootGroupSize;

		protected virtual int MaxRootGroupSize => def.maxRootGroupSize;

		protected virtual int MinOverallSize => def.minSize;

		protected virtual int MaxOverallSize => def.maxSize;

		protected virtual int MaxSpaceBetweenRootGroups => def.maxSpaceBetweenRootGroups;

		protected abstract bool IsRoot(int tile);

		protected virtual bool CanTraverse(int tile, out bool ifRootThenRootGroupSizeMustMatch)
		{
			ifRootThenRootGroupSizeMustMatch = false;
			return true;
		}

		protected virtual bool IsMember(int tile, out bool ifRootThenRootGroupSizeMustMatch)
		{
			ifRootThenRootGroupSizeMustMatch = false;
			return Find.WorldGrid[tile].feature == null;
		}

		public override void GenerateWhereAppropriate()
		{
			CalculateRootTiles();
			CalculateRootsWithAreaInBetween();
			CalculateContiguousGroups();
		}

		private void CalculateRootTiles()
		{
			roots.Clear();
			int tilesCount = Find.WorldGrid.TilesCount;
			for (int i = 0; i < tilesCount; i++)
			{
				if (IsRoot(i))
				{
					roots.Add(i);
				}
			}
			rootsSet.Clear();
			rootsSet.AddRange(roots);
		}

		private void CalculateRootsWithAreaInBetween()
		{
			rootsWithAreaInBetween.Clear();
			rootsWithAreaInBetween.AddRange(roots);
			GenPlanetMorphology.Close(rootsWithAreaInBetween, MaxSpaceBetweenRootGroups);
			rootsWithAreaInBetweenSet.Clear();
			rootsWithAreaInBetweenSet.AddRange(rootsWithAreaInBetween);
		}

		private void CalculateContiguousGroups()
		{
			WorldFloodFiller worldFloodFiller = Find.WorldFloodFiller;
			WorldGrid worldGrid = Find.WorldGrid;
			int minRootGroupSize = MinRootGroupSize;
			int maxRootGroupSize = MaxRootGroupSize;
			int minOverallSize = MinOverallSize;
			int maxOverallSize = MaxOverallSize;
			int minRootGroupsInCluster = MinRootGroupsInCluster;
			FeatureWorker.ClearVisited();
			FeatureWorker.ClearGroupSizes();
			FeatureWorker.ClearGroupIDs();
			for (int i = 0; i < roots.Count; i++)
			{
				int num = roots[i];
				if (FeatureWorker.visited[num])
				{
					continue;
				}
				bool anyMember = false;
				tmpGroup.Clear();
				worldFloodFiller.FloodFill(num, (int x) => rootsSet.Contains(x), delegate(int x)
				{
					FeatureWorker.visited[x] = true;
					tmpGroup.Add(x);
					if (!anyMember && IsMember(x, out bool _))
					{
						anyMember = true;
					}
				});
				for (int j = 0; j < tmpGroup.Count; j++)
				{
					FeatureWorker.groupSize[tmpGroup[j]] = tmpGroup.Count;
					if (anyMember)
					{
						FeatureWorker.groupID[tmpGroup[j]] = i + 1;
					}
				}
			}
			FeatureWorker.ClearVisited();
			for (int k = 0; k < roots.Count; k++)
			{
				int num2 = roots[k];
				if (FeatureWorker.visited[num2] || FeatureWorker.groupSize[num2] < minRootGroupSize || FeatureWorker.groupSize[num2] > maxRootGroupSize || FeatureWorker.groupSize[num2] > maxOverallSize)
				{
					continue;
				}
				currentGroup.Clear();
				visitedValidGroupIDs.Clear();
				worldFloodFiller.FloodFill(num2, delegate(int x)
				{
					if (!rootsWithAreaInBetweenSet.Contains(x))
					{
						return false;
					}
					if (!CanTraverse(x, out bool ifRootThenRootGroupSizeMustMatch2))
					{
						return false;
					}
					return (!ifRootThenRootGroupSizeMustMatch2 || !rootsSet.Contains(x) || (FeatureWorker.groupSize[x] >= minRootGroupSize && FeatureWorker.groupSize[x] <= maxRootGroupSize)) ? true : false;
				}, delegate(int x)
				{
					FeatureWorker.visited[x] = true;
					currentGroup.Add(x);
					if (FeatureWorker.groupID[x] != 0 && FeatureWorker.groupSize[x] >= minRootGroupSize && FeatureWorker.groupSize[x] <= maxRootGroupSize)
					{
						visitedValidGroupIDs.Add(FeatureWorker.groupID[x]);
					}
				});
				if (currentGroup.Count < minOverallSize || currentGroup.Count > maxOverallSize || visitedValidGroupIDs.Count < minRootGroupsInCluster || (!def.canTouchWorldEdge && currentGroup.Any((int x) => worldGrid.IsOnEdge(x))))
				{
					continue;
				}
				currentGroupMembers.Clear();
				for (int l = 0; l < currentGroup.Count; l++)
				{
					int num3 = currentGroup[l];
					if (IsMember(num3, out bool ifRootThenRootGroupSizeMustMatch) && (!ifRootThenRootGroupSizeMustMatch || !rootsSet.Contains(num3) || (FeatureWorker.groupSize[num3] >= minRootGroupSize && FeatureWorker.groupSize[num3] <= maxRootGroupSize)))
					{
						currentGroupMembers.Add(currentGroup[l]);
					}
				}
				if (currentGroupMembers.Count >= minOverallSize)
				{
					if (currentGroup.Any((int x) => worldGrid[x].feature == null))
					{
						currentGroup.RemoveAll((int x) => worldGrid[x].feature != null);
					}
					AddFeature(currentGroupMembers, currentGroup);
				}
			}
		}
	}
}
