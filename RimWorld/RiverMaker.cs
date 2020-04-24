using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace RimWorld
{
	public class RiverMaker
	{
		private ModuleBase generator;

		private ModuleBase coordinateX;

		private ModuleBase coordinateZ;

		private ModuleBase shallowizer;

		private float surfaceLevel;

		private float shallowFactor = 0.2f;

		private List<IntVec3> lhs = new List<IntVec3>();

		private List<IntVec3> rhs = new List<IntVec3>();

		public RiverMaker(Vector3 center, float angle, RiverDef riverDef)
		{
			surfaceLevel = riverDef.widthOnMap / 2f;
			coordinateX = new AxisAsValueX();
			coordinateZ = new AxisAsValueZ();
			coordinateX = new Rotate(0.0, 0f - angle, 0.0, coordinateX);
			coordinateZ = new Rotate(0.0, 0f - angle, 0.0, coordinateZ);
			coordinateX = new Translate(0f - center.x, 0.0, 0f - center.z, coordinateX);
			coordinateZ = new Translate(0f - center.x, 0.0, 0f - center.z, coordinateZ);
			ModuleBase moduleBase = new Perlin(0.029999999329447746, 2.0, 0.5, 3, Rand.Range(0, int.MaxValue), QualityMode.Medium);
			ModuleBase moduleBase2 = new Perlin(0.029999999329447746, 2.0, 0.5, 3, Rand.Range(0, int.MaxValue), QualityMode.Medium);
			ModuleBase moduleBase3 = new Const(8.0);
			moduleBase = new Multiply(moduleBase, moduleBase3);
			moduleBase2 = new Multiply(moduleBase2, moduleBase3);
			coordinateX = new Displace(coordinateX, moduleBase, new Const(0.0), moduleBase2);
			coordinateZ = new Displace(coordinateZ, moduleBase, new Const(0.0), moduleBase2);
			generator = coordinateX;
			shallowizer = new Perlin(0.029999999329447746, 2.0, 0.5, 3, Rand.Range(0, int.MaxValue), QualityMode.Medium);
			shallowizer = new Abs(shallowizer);
		}

		public TerrainDef TerrainAt(IntVec3 loc, bool recordForValidation = false)
		{
			float value = generator.GetValue(loc);
			float num = surfaceLevel - Mathf.Abs(value);
			if (num > 2f && shallowizer.GetValue(loc) > shallowFactor)
			{
				return TerrainDefOf.WaterMovingChestDeep;
			}
			if (num > 0f)
			{
				if (recordForValidation)
				{
					if (value < 0f)
					{
						lhs.Add(loc);
					}
					else
					{
						rhs.Add(loc);
					}
				}
				return TerrainDefOf.WaterMovingShallow;
			}
			return null;
		}

		public Vector3 WaterCoordinateAt(IntVec3 loc)
		{
			return new Vector3(coordinateX.GetValue(loc), 0f, coordinateZ.GetValue(loc));
		}

		public void ValidatePassage(Map map)
		{
			IntVec3 intVec = lhs.Where((IntVec3 loc) => loc.InBounds(map) && loc.GetTerrain(map) == TerrainDefOf.WaterMovingShallow).RandomElementWithFallback(IntVec3.Invalid);
			IntVec3 intVec2 = rhs.Where((IntVec3 loc) => loc.InBounds(map) && loc.GetTerrain(map) == TerrainDefOf.WaterMovingShallow).RandomElementWithFallback(IntVec3.Invalid);
			if (intVec == IntVec3.Invalid || intVec2 == IntVec3.Invalid)
			{
				Log.Error("Failed to find river edges in order to verify passability");
				return;
			}
			while (true)
			{
				if (!map.reachability.CanReach(intVec, intVec2, PathEndMode.OnCell, TraverseMode.PassAllDestroyableThings))
				{
					if (shallowFactor > 1f)
					{
						break;
					}
					shallowFactor += 0.1f;
					foreach (IntVec3 allCell in map.AllCells)
					{
						if (allCell.GetTerrain(map) == TerrainDefOf.WaterMovingChestDeep && shallowizer.GetValue(allCell) <= shallowFactor)
						{
							map.terrainGrid.SetTerrain(allCell, TerrainDefOf.WaterMovingShallow);
						}
					}
					continue;
				}
				return;
			}
			Log.Error("Failed to make river shallow enough for passability");
		}
	}
}
