using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_CultivatedPlants : SymbolResolver
{
	private const float MinPlantGrowth = 0.2f;

	private static List<Thing> tmpThings = new List<Thing>();

	public override bool CanResolve(ResolveParams rp)
	{
		if (base.CanResolve(rp))
		{
			if (rp.cultivatedPlantDef == null)
			{
				return DeterminePlantDef(rp.rect) != null;
			}
			return true;
		}
		return false;
	}

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		ThingDef thingDef = rp.cultivatedPlantDef ?? DeterminePlantDef(rp.rect);
		if (thingDef == null)
		{
			return;
		}
		float growth = rp.fixedCulativedPlantGrowth ?? Rand.Range(0.2f, 1f);
		int age = (thingDef.plant.LimitedLifespan ? Rand.Range(0, Mathf.Max(thingDef.plant.LifespanTicks - 2500, 0)) : 0);
		List<Thing> list = new List<Thing>();
		foreach (IntVec3 c in rp.rect)
		{
			if (!c.InBounds(map) || list.Any((Thing p) => p.def.plant.blockAdjacentSow && c.IsAdjacentToCardinalOrInside(p.OccupiedRect())))
			{
				continue;
			}
			float num = map.fertilityGrid.FertilityAt(c);
			if ((thingDef.plant.completelyIgnoreFertility || !(num < thingDef.plant.fertilityMin)) && TryDestroyBlockingThingsAt(c))
			{
				Plant plant = (Plant)GenSpawn.Spawn(thingDef, c, map);
				list.Add(plant);
				plant.Growth = growth;
				if (plant.def.plant.LimitedLifespan)
				{
					plant.Age = age;
				}
			}
		}
	}

	public static ThingDef DeterminePlantDef(CellRect rect)
	{
		Map map = BaseGen.globalSettings.map;
		float minFertility = float.MaxValue;
		bool flag = false;
		foreach (IntVec3 item in rect)
		{
			float num = map.fertilityGrid.FertilityAt(item);
			if (!(num <= 0f))
			{
				flag = true;
				minFertility = Mathf.Min(minFertility, num);
			}
		}
		if (!flag)
		{
			return null;
		}
		if (DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => x.category == ThingCategory.Plant && x.plant.Sowable && !x.plant.IsTree && !x.plant.cavePlant && !x.plant.diesToLight && (x.plant.completelyIgnoreFertility || x.plant.fertilityMin <= minFertility) && x.plant.minGrowthTemperature <= map.mapTemperature.OutdoorTemp && x.plant.maxGrowthTemperature >= map.mapTemperature.OutdoorTemp && x.plant.Harvestable).TryRandomElement(out var result))
		{
			return result;
		}
		return null;
	}

	private bool TryDestroyBlockingThingsAt(IntVec3 c)
	{
		Map map = BaseGen.globalSettings.map;
		tmpThings.Clear();
		tmpThings.AddRange(c.GetThingList(map));
		for (int i = 0; i < tmpThings.Count; i++)
		{
			if (!(tmpThings[i] is Pawn) && !tmpThings[i].def.destroyable)
			{
				tmpThings.Clear();
				return false;
			}
		}
		for (int j = 0; j < tmpThings.Count; j++)
		{
			if (!(tmpThings[j] is Pawn))
			{
				tmpThings[j].Destroy();
			}
		}
		tmpThings.Clear();
		return true;
	}
}
