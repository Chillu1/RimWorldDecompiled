using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_GeothermalVents : GenStep_ScatterThings
{
	public List<BiomePlantRecord> floraToScatter;

	public SimpleCurve densityByDistance;

	[Unsaved(false)]
	private List<IntVec3> geothermalVents = new List<IntVec3>();

	public override int SeedPart => 2140896;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		base.Generate(map, parms);
		float x = densityByDistance.MaxBy((CurvePoint p) => p.x).x;
		ThingGrid thingGrid = map.thingGrid;
		foreach (IntVec3 geothermalVent in geothermalVents)
		{
			foreach (IntVec3 item in GenRadial.RadialCellsAround(geothermalVent, x, useCenter: false))
			{
				if (thingGrid.ThingAt(item, ThingDefOf.GeothermalVent) == null)
				{
					float lengthHorizontal = (item - geothermalVent).LengthHorizontal;
					if (Rand.Chance(densityByDistance.Evaluate(lengthHorizontal)))
					{
						DoPlaceRandomFloraAt(item, map);
					}
				}
			}
		}
		geothermalVents.Clear();
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int stackCount = 1)
	{
		geothermalVents.Add(loc);
		base.ScatterAt(loc, map, parms, stackCount);
	}

	private void DoPlaceRandomFloraAt(IntVec3 pos, Map map)
	{
		ThingDef plant = floraToScatter.RandomElementByWeight((BiomePlantRecord f) => f.commonality).plant;
		if ((bool)plant.CanEverPlantAt(pos, map, out var _, canWipePlantsExceptTree: false, checkMapTemperature: true, writeNoReason: true))
		{
			Plant plant2 = (Plant)ThingMaker.MakeThing(plant);
			plant2.Growth = Mathf.Clamp01(WildPlantSpawner.InitialGrowthRandomRange.RandomInRange);
			if (plant2.def.plant.LimitedLifespan)
			{
				plant2.Age = Rand.Range(0, Mathf.Max(plant2.def.plant.LifespanTicks - 50, 0));
			}
			GenSpawn.Spawn(plant2, pos, map);
		}
	}
}
