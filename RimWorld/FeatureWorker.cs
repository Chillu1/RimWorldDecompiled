using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class FeatureWorker
	{
		public FeatureDef def;

		protected static bool[] visited;

		protected static int[] groupSize;

		protected static int[] groupID;

		private static List<int> tmpNeighbors = new List<int>();

		private static HashSet<int> tmpTilesForTextDrawPosCalculationSet = new HashSet<int>();

		private static List<int> tmpEdgeTiles = new List<int>();

		private static List<Pair<int, int>> tmpTraversedTiles = new List<Pair<int, int>>();

		public abstract void GenerateWhereAppropriate();

		protected void AddFeature(List<int> members, List<int> tilesForTextDrawPosCalculation)
		{
			WorldFeature worldFeature = new WorldFeature();
			worldFeature.uniqueID = Find.UniqueIDsManager.GetNextWorldFeatureID();
			worldFeature.def = def;
			worldFeature.name = NameGenerator.GenerateName(def.nameMaker, Find.WorldFeatures.features.Select((WorldFeature x) => x.name), appendNumberIfNameUsed: false, "r_name");
			WorldGrid worldGrid = Find.WorldGrid;
			for (int i = 0; i < members.Count; i++)
			{
				worldGrid[members[i]].feature = worldFeature;
			}
			AssignBestDrawPos(worldFeature, tilesForTextDrawPosCalculation);
			Find.WorldFeatures.features.Add(worldFeature);
		}

		private void AssignBestDrawPos(WorldFeature newFeature, List<int> tilesForTextDrawPosCalculation)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			tmpEdgeTiles.Clear();
			tmpTilesForTextDrawPosCalculationSet.Clear();
			tmpTilesForTextDrawPosCalculationSet.AddRange(tilesForTextDrawPosCalculation);
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < tilesForTextDrawPosCalculation.Count; i++)
			{
				int num = tilesForTextDrawPosCalculation[i];
				zero += worldGrid.GetTileCenter(num);
				bool flag = worldGrid.IsOnEdge(num);
				if (!flag)
				{
					worldGrid.GetTileNeighbors(num, tmpNeighbors);
					for (int j = 0; j < tmpNeighbors.Count; j++)
					{
						if (!tmpTilesForTextDrawPosCalculationSet.Contains(tmpNeighbors[j]))
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					tmpEdgeTiles.Add(num);
				}
			}
			zero /= (float)tilesForTextDrawPosCalculation.Count;
			if (!tmpEdgeTiles.Any())
			{
				tmpEdgeTiles.Add(tilesForTextDrawPosCalculation.RandomElement());
			}
			int bestTileDist = 0;
			tmpTraversedTiles.Clear();
			Find.WorldFloodFiller.FloodFill(-1, (int x) => tmpTilesForTextDrawPosCalculationSet.Contains(x), delegate(int tile, int traversalDist)
			{
				tmpTraversedTiles.Add(new Pair<int, int>(tile, traversalDist));
				bestTileDist = traversalDist;
				return false;
			}, int.MaxValue, tmpEdgeTiles);
			int num2 = -1;
			float num3 = -1f;
			for (int k = 0; k < tmpTraversedTiles.Count; k++)
			{
				if (tmpTraversedTiles[k].Second == bestTileDist)
				{
					float sqrMagnitude = (worldGrid.GetTileCenter(tmpTraversedTiles[k].First) - zero).sqrMagnitude;
					if (num2 == -1 || sqrMagnitude < num3)
					{
						num2 = tmpTraversedTiles[k].First;
						num3 = sqrMagnitude;
					}
				}
			}
			float maxDrawSizeInTiles = (float)bestTileDist * 2f * 1.2f;
			newFeature.drawCenter = worldGrid.GetTileCenter(num2);
			newFeature.maxDrawSizeInTiles = maxDrawSizeInTiles;
		}

		protected static void ClearVisited()
		{
			ClearOrCreate(ref visited);
		}

		protected static void ClearGroupSizes()
		{
			ClearOrCreate(ref groupSize);
		}

		protected static void ClearGroupIDs()
		{
			ClearOrCreate(ref groupID);
		}

		private static void ClearOrCreate<T>(ref T[] array)
		{
			int tilesCount = Find.WorldGrid.TilesCount;
			if (array == null || array.Length != tilesCount)
			{
				array = new T[tilesCount];
			}
			else
			{
				Array.Clear(array, 0, array.Length);
			}
		}
	}
}
