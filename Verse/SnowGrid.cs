using UnityEngine;

namespace Verse
{
	public sealed class SnowGrid : IExposable
	{
		private Map map;

		private float[] depthGrid;

		private double totalDepth;

		public const float MaxDepth = 1f;

		internal float[] DepthGridDirect_Unsafe => depthGrid;

		public float TotalDepth => (float)totalDepth;

		public SnowGrid(Map map)
		{
			this.map = map;
			depthGrid = new float[map.cellIndices.NumGridCells];
		}

		public void ExposeData()
		{
			MapExposeUtility.ExposeUshort(map, (IntVec3 c) => SnowFloatToShort(GetDepth(c)), delegate(IntVec3 c, ushort val)
			{
				depthGrid[map.cellIndices.CellToIndex(c)] = SnowShortToFloat(val);
			}, "depthGrid");
		}

		private static ushort SnowFloatToShort(float depth)
		{
			depth = Mathf.Clamp(depth, 0f, 1f);
			depth *= 65535f;
			return (ushort)Mathf.RoundToInt(depth);
		}

		private static float SnowShortToFloat(ushort depth)
		{
			return (float)(int)depth / 65535f;
		}

		private bool CanHaveSnow(int ind)
		{
			Building building = map.edificeGrid[ind];
			if (building != null && !CanCoexistWithSnow(building.def))
			{
				return false;
			}
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(ind);
			if (terrainDef != null && !terrainDef.holdSnow)
			{
				return false;
			}
			return true;
		}

		public static bool CanCoexistWithSnow(ThingDef def)
		{
			if (def.category == ThingCategory.Building && def.Fillage == FillCategory.Full)
			{
				return false;
			}
			return true;
		}

		public void AddDepth(IntVec3 c, float depthToAdd)
		{
			int num = map.cellIndices.CellToIndex(c);
			float num2 = depthGrid[num];
			if ((num2 <= 0f && depthToAdd < 0f) || (num2 >= 0.999f && depthToAdd > 1f))
			{
				return;
			}
			if (!CanHaveSnow(num))
			{
				depthGrid[num] = 0f;
				return;
			}
			float value = num2 + depthToAdd;
			value = Mathf.Clamp(value, 0f, 1f);
			float num3 = value - num2;
			totalDepth += num3;
			if (Mathf.Abs(num3) > 0.0001f)
			{
				depthGrid[num] = value;
				CheckVisualOrPathCostChange(c, num2, value);
			}
		}

		public void SetDepth(IntVec3 c, float newDepth)
		{
			int num = map.cellIndices.CellToIndex(c);
			if (!CanHaveSnow(num))
			{
				depthGrid[num] = 0f;
				return;
			}
			newDepth = Mathf.Clamp(newDepth, 0f, 1f);
			float num2 = depthGrid[num];
			depthGrid[num] = newDepth;
			float num3 = newDepth - num2;
			totalDepth += num3;
			CheckVisualOrPathCostChange(c, num2, newDepth);
		}

		private void CheckVisualOrPathCostChange(IntVec3 c, float oldDepth, float newDepth)
		{
			if (!Mathf.Approximately(oldDepth, newDepth))
			{
				if (Mathf.Abs(oldDepth - newDepth) > 0.15f || Rand.Value < 0.0125f)
				{
					map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Snow, regenAdjacentCells: true, regenAdjacentSections: false);
					map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Things, regenAdjacentCells: true, regenAdjacentSections: false);
				}
				else if (newDepth == 0f)
				{
					map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Snow, regenAdjacentCells: true, regenAdjacentSections: false);
				}
				if (SnowUtility.GetSnowCategory(oldDepth) != SnowUtility.GetSnowCategory(newDepth))
				{
					map.pathGrid.RecalculatePerceivedPathCostAt(c);
				}
			}
		}

		public float GetDepth(IntVec3 c)
		{
			if (!c.InBounds(map))
			{
				return 0f;
			}
			return depthGrid[map.cellIndices.CellToIndex(c)];
		}

		public SnowCategory GetCategory(IntVec3 c)
		{
			return SnowUtility.GetSnowCategory(GetDepth(c));
		}
	}
}
