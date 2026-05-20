using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class GenStep_RocksFromGrid : GenStep
{
	private class RoofThreshold
	{
		public RoofDef roofDef;

		public float minGridVal;
	}

	private float maxMineableValue = float.MaxValue;

	private bool ignoreMaxIfGravship;

	private float? overrideBlotchesPer10kCells;

	private const int MinRoofedCellsPerGroup = 20;

	public override int SeedPart => 1182952823;

	public static ThingDef RockDefAt(IntVec3 c)
	{
		ThingDef thingDef = null;
		float num = -999999f;
		for (int i = 0; i < RockNoises.rockNoises.Count; i++)
		{
			float value = RockNoises.rockNoises[i].noise.GetValue(c);
			if (value > num)
			{
				thingDef = RockNoises.rockNoises[i].rockDef;
				num = value;
			}
		}
		if (thingDef == null)
		{
			IntVec3 intVec = c;
			Log.ErrorOnce("Did not get rock def to generate at " + intVec.ToString(), 50812);
			thingDef = ThingDefOf.Sandstone;
		}
		return thingDef;
	}

	public override void Generate(Map map, GenStepParams parms)
	{
		if (map.TileInfo.WaterCovered)
		{
			return;
		}
		map.regionAndRoomUpdater.Enabled = false;
		float num = 0.7f;
		List<RoofThreshold> list = new List<RoofThreshold>();
		RoofThreshold roofThreshold = new RoofThreshold();
		roofThreshold.roofDef = RoofDefOf.RoofRockThick;
		roofThreshold.minGridVal = num * 1.14f;
		list.Add(roofThreshold);
		RoofThreshold roofThreshold2 = new RoofThreshold();
		roofThreshold2.roofDef = RoofDefOf.RoofRockThin;
		roofThreshold2.minGridVal = num * 1.04f;
		list.Add(roofThreshold2);
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		MapGenFloatGrid caves = MapGenerator.Caves;
		foreach (IntVec3 allCell in map.AllCells)
		{
			float num2 = elevation[allCell];
			if (!(num2 > num))
			{
				continue;
			}
			if (caves[allCell] <= 0f)
			{
				GenSpawn.Spawn(RockDefAt(allCell), allCell, map);
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (num2 > list[i].minGridVal)
				{
					map.roofGrid.SetRoof(allCell, list[i].roofDef);
					break;
				}
			}
		}
		BoolGrid visited = new BoolGrid(map);
		List<IntVec3> toRemove = new List<IntVec3>();
		foreach (IntVec3 allCell2 in map.AllCells)
		{
			if (visited[allCell2] || !IsNaturalRoofAt(allCell2, map))
			{
				continue;
			}
			toRemove.Clear();
			map.floodFiller.FloodFill(allCell2, (IntVec3 x) => IsNaturalRoofAt(x, map), delegate(IntVec3 x)
			{
				visited[x] = true;
				toRemove.Add(x);
			});
			if (toRemove.Count < 20)
			{
				for (int num3 = 0; num3 < toRemove.Count; num3++)
				{
					map.roofGrid.SetRoof(toRemove[num3], null);
				}
			}
		}
		GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
		if (!ModsConfig.OdysseyActive || !ignoreMaxIfGravship || !map.wasSpawnedViaGravShipLanding)
		{
			genStep_ScatterLumpsMineable.maxValue = maxMineableValue;
		}
		genStep_ScatterLumpsMineable.useNomadicMineables = true;
		float num4 = GetResourceBlotchesPer10KCellsForMap(map);
		if (overrideBlotchesPer10kCells.HasValue)
		{
			num4 = overrideBlotchesPer10kCells.Value;
		}
		genStep_ScatterLumpsMineable.countPer10kCellsRange = new FloatRange(num4, num4);
		genStep_ScatterLumpsMineable.Generate(map, parms);
		map.regionAndRoomUpdater.Enabled = true;
	}

	public static float GetResourceBlotchesPer10KCellsForMap(Map map)
	{
		float result = 10f;
		switch (map.TileInfo.HillinessForOreGeneration)
		{
		case Hilliness.Flat:
			result = 4f;
			break;
		case Hilliness.SmallHills:
			result = 8f;
			break;
		case Hilliness.LargeHills:
			result = 11f;
			break;
		case Hilliness.Mountainous:
			result = 15f;
			break;
		case Hilliness.Impassable:
			result = 16f;
			break;
		}
		return result;
	}

	private bool IsNaturalRoofAt(IntVec3 c, Map map)
	{
		if (c.Roofed(map))
		{
			return c.GetRoof(map).isNatural;
		}
		return false;
	}
}
