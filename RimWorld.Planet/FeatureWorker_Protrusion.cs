using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public abstract class FeatureWorker_Protrusion : FeatureWorker
	{
		private List<int> roots = new List<int>();

		private HashSet<int> rootsSet = new HashSet<int>();

		private List<int> rootsWithoutSmallPassages = new List<int>();

		private HashSet<int> rootsWithoutSmallPassagesSet = new HashSet<int>();

		private List<int> currentGroup = new List<int>();

		private List<int> currentGroupMembers = new List<int>();

		private static List<int> tmpGroup = new List<int>();

		protected virtual int MinSize => def.minSize;

		protected virtual int MaxSize => def.maxSize;

		protected virtual int MaxPassageWidth => def.maxPassageWidth;

		protected virtual float MaxPctOfWholeArea => def.maxPctOfWholeArea;

		protected abstract bool IsRoot(int tile);

		protected virtual bool IsMember(int tile)
		{
			return Find.WorldGrid[tile].feature == null;
		}

		public override void GenerateWhereAppropriate()
		{
			CalculateRoots();
			CalculateRootsWithoutSmallPassages();
			CalculateContiguousGroups();
		}

		private void CalculateRoots()
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

		private void CalculateRootsWithoutSmallPassages()
		{
			rootsWithoutSmallPassages.Clear();
			rootsWithoutSmallPassages.AddRange(roots);
			GenPlanetMorphology.Open(rootsWithoutSmallPassages, MaxPassageWidth);
			rootsWithoutSmallPassagesSet.Clear();
			rootsWithoutSmallPassagesSet.AddRange(rootsWithoutSmallPassages);
		}

		private void CalculateContiguousGroups()
		{
			WorldGrid worldGrid = Find.WorldGrid;
			WorldFloodFiller worldFloodFiller = Find.WorldFloodFiller;
			int minSize = MinSize;
			int maxSize = MaxSize;
			float maxPctOfWholeArea = MaxPctOfWholeArea;
			int maxPassageWidth = MaxPassageWidth;
			FeatureWorker.ClearVisited();
			FeatureWorker.ClearGroupSizes();
			for (int i = 0; i < roots.Count; i++)
			{
				int num = roots[i];
				if (!FeatureWorker.visited[num])
				{
					tmpGroup.Clear();
					worldFloodFiller.FloodFill(num, (int x) => rootsSet.Contains(x), delegate(int x)
					{
						FeatureWorker.visited[x] = true;
						tmpGroup.Add(x);
					});
					for (int j = 0; j < tmpGroup.Count; j++)
					{
						FeatureWorker.groupSize[tmpGroup[j]] = tmpGroup.Count;
					}
				}
			}
			FeatureWorker.ClearVisited();
			for (int k = 0; k < rootsWithoutSmallPassages.Count; k++)
			{
				int num2 = rootsWithoutSmallPassages[k];
				if (FeatureWorker.visited[num2])
				{
					continue;
				}
				currentGroup.Clear();
				worldFloodFiller.FloodFill(num2, (int x) => rootsWithoutSmallPassagesSet.Contains(x), delegate(int x)
				{
					FeatureWorker.visited[x] = true;
					currentGroup.Add(x);
				});
				if (currentGroup.Count < minSize)
				{
					continue;
				}
				GenPlanetMorphology.Dilate(currentGroup, maxPassageWidth * 2, (int x) => rootsSet.Contains(x));
				if (currentGroup.Count > maxSize || (float)currentGroup.Count / (float)FeatureWorker.groupSize[num2] > maxPctOfWholeArea || (!def.canTouchWorldEdge && currentGroup.Any((int x) => worldGrid.IsOnEdge(x))))
				{
					continue;
				}
				currentGroupMembers.Clear();
				for (int l = 0; l < currentGroup.Count; l++)
				{
					if (IsMember(currentGroup[l]))
					{
						currentGroupMembers.Add(currentGroup[l]);
					}
				}
				if (currentGroupMembers.Count < minSize)
				{
					continue;
				}
				if (currentGroup.Any((int x) => worldGrid[x].feature == null))
				{
					currentGroup.RemoveAll((int x) => worldGrid[x].feature != null);
				}
				AddFeature(currentGroupMembers, currentGroup);
			}
		}
	}
}
