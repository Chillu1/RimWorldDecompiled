using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public abstract class GenStep_Scatterer : GenStep
	{
		public int count = -1;

		public FloatRange countPer10kCellsRange = FloatRange.Zero;

		public bool nearPlayerStart;

		public bool nearMapCenter;

		public float minSpacing = 10f;

		public bool spotMustBeStandable;

		public int minDistToPlayerStart;

		public int minEdgeDist;

		public int extraNoBuildEdgeDist;

		public List<ScattererValidator> validators = new List<ScattererValidator>();

		public bool allowInWaterBiome = true;

		public bool allowFoggedPositions = true;

		public bool warnOnFail = true;

		[Unsaved(false)]
		protected List<IntVec3> usedSpots = new List<IntVec3>();

		private const int ScatterNearPlayerRadius = 20;

		public override void Generate(Map map, GenStepParams parms)
		{
			if (!allowInWaterBiome && map.TileInfo.WaterCovered)
			{
				return;
			}
			int num = CalculateFinalCount(map);
			for (int i = 0; i < num; i++)
			{
				if (!TryFindScatterCell(map, out var result))
				{
					return;
				}
				ScatterAt(result, map, parms);
				usedSpots.Add(result);
			}
			usedSpots.Clear();
		}

		protected virtual bool TryFindScatterCell(Map map, out IntVec3 result)
		{
			if (nearMapCenter)
			{
				if (RCellFinder.TryFindRandomCellNearWith(map.Center, (IntVec3 x) => CanScatterAt(x, map), map, out result, 3))
				{
					return true;
				}
			}
			else
			{
				if (nearPlayerStart)
				{
					result = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, map, 20, (IntVec3 x) => CanScatterAt(x, map));
					return true;
				}
				if (CellFinderLoose.TryFindRandomNotEdgeCellWith(5, (IntVec3 x) => CanScatterAt(x, map), map, out result))
				{
					return true;
				}
			}
			if (warnOnFail)
			{
				Log.Warning("Scatterer " + ToString() + " could not find cell to generate at.");
			}
			return false;
		}

		protected abstract void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1);

		protected virtual bool CanScatterAt(IntVec3 loc, Map map)
		{
			if (extraNoBuildEdgeDist > 0 && loc.CloseToEdge(map, extraNoBuildEdgeDist + 10))
			{
				return false;
			}
			if (minEdgeDist > 0 && loc.CloseToEdge(map, minEdgeDist))
			{
				return false;
			}
			if (NearUsedSpot(loc, minSpacing))
			{
				return false;
			}
			if ((map.Center - loc).LengthHorizontalSquared < minDistToPlayerStart * minDistToPlayerStart)
			{
				return false;
			}
			if (spotMustBeStandable && !loc.Standable(map))
			{
				return false;
			}
			if (!allowFoggedPositions && loc.Fogged(map))
			{
				return false;
			}
			if (validators != null)
			{
				for (int i = 0; i < validators.Count; i++)
				{
					if (!validators[i].Allows(loc, map))
					{
						return false;
					}
				}
			}
			return true;
		}

		protected bool NearUsedSpot(IntVec3 c, float dist)
		{
			for (int i = 0; i < usedSpots.Count; i++)
			{
				if ((float)(usedSpots[i] - c).LengthHorizontalSquared <= dist * dist)
				{
					return true;
				}
			}
			return false;
		}

		protected int CalculateFinalCount(Map map)
		{
			if (count < 0)
			{
				return CountFromPer10kCells(countPer10kCellsRange.RandomInRange, map);
			}
			return count;
		}

		public static int CountFromPer10kCells(float countPer10kCells, Map map, int mapSize = -1)
		{
			if (mapSize < 0)
			{
				mapSize = map.Size.x;
			}
			int num = Mathf.RoundToInt(10000f / countPer10kCells);
			return Mathf.RoundToInt((float)(mapSize * mapSize) / (float)num);
		}

		public void ForceScatterAt(IntVec3 loc, Map map)
		{
			ScatterAt(loc, map, default(GenStepParams));
		}
	}
}
