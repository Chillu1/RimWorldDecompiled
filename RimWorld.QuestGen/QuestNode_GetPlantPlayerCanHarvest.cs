using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetPlantPlayerCanHarvest : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeHarvestItemDefAs;

	[NoTranslate]
	public SlateRef<string> storeHarvestItemCountAs;

	[NoTranslate]
	public SlateRef<string> storeGrowDaysAs;

	public SlateRef<int> maxPlantGrowDays;

	public SlateRef<SimpleCurve> pointsToRequiredWorkCurve;

	public SlateRef<FloatRange?> workRandomFactorRange;

	private const float TemperatureBuffer = 5f;

	private const int TemperatureCheckDays = 15;

	protected override bool TestRunInt(Slate slate)
	{
		return DoWork(slate);
	}

	protected override void RunInt()
	{
		DoWork(QuestGen.slate);
	}

	private bool DoWork(Slate slate)
	{
		Map map = slate.Get<Map>("map");
		if (map == null)
		{
			return false;
		}
		float x = slate.Get("points", 0f);
		float highestTemp;
		float lowestTemp;
		float num = (highestTemp = (lowestTemp = Find.World.tileTemperatures.GetSeasonalTemp(map.Tile)));
		int ticksAbs = GenTicks.TicksAbs;
		for (int i = 0; i < 15; i++)
		{
			int absTick = ticksAbs + 60000 * i;
			float num2 = num + Find.World.tileTemperatures.OffsetFromDailyRandomVariation(map.Tile, absTick);
			if (num2 < lowestTemp)
			{
				lowestTemp = num2;
			}
			if (num2 > highestTemp)
			{
				highestTemp = num2;
			}
		}
		if (!DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.category == ThingCategory.Plant && !def.plant.cavePlant && def.plant.Sowable && def.plant.harvestedThingDef != null && def.plant.growDays <= (float)maxPlantGrowDays.GetValue(slate) && def.plant.minGrowthTemperature <= lowestTemp + 5f && def.plant.maxGrowthTemperature >= highestTemp - 5f && Command_SetPlantToGrow.IsPlantAvailable(def, map)).TryRandomElement(out var result))
		{
			return false;
		}
		SimpleCurve value = pointsToRequiredWorkCurve.GetValue(slate);
		float randomInRange = (workRandomFactorRange.GetValue(slate) ?? FloatRange.One).RandomInRange;
		float num3 = value.Evaluate(x) * randomInRange;
		float num4 = (result.plant.sowWork + result.plant.harvestWork) / result.plant.harvestYield;
		int a = GenMath.RoundRandom(num3 / num4);
		a = Mathf.Max(a, 1);
		slate.Set(storeHarvestItemDefAs.GetValue(slate), result.plant.harvestedThingDef);
		slate.Set(storeHarvestItemCountAs.GetValue(slate), a);
		if (storeGrowDaysAs.GetValue(slate) != null)
		{
			slate.Set(storeGrowDaysAs.GetValue(slate), result.plant.growDays);
		}
		return true;
	}
}
