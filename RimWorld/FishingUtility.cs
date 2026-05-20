using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class FishingUtility
{
	public const float FishPopulationRegenerationFactorPerDay = 0.025f;

	public const int BaseFishingDurationTicks = 7500;

	public const int AnimalFishingDurationTicks = 1500;

	public const float ChanceToCatchUncommonFish = 0.05f;

	public const float ChanceForRareCatch = 0.01f;

	private const int RareCatchCooldownTicks = 300000;

	public const float ChanceForNegativeCatch = 0.02f;

	private const int NegativeCatchCooldownTicks = 120000;

	public const int MinFishPopulationForFishing = 10;

	public const int MinFishPopulationPerWaterBody = 20;

	public static readonly SimpleCurve FishPopulationFactorPerBodySizeCurve = new SimpleCurve
	{
		new CurvePoint(74.9f, 0f),
		new CurvePoint(75f, 0.03f),
		new CurvePoint(400f, 0.04f),
		new CurvePoint(1600f, 0.1f),
		new CurvePoint(3200f, 0.24f),
		new CurvePoint(6000f, 0.8f),
		new CurvePoint(20000f, 1f)
	};

	public static readonly SimpleCurve ChanceForCommonFishFromWaterBodySizeCurve = new SimpleCurve
	{
		new CurvePoint(74.9f, 0f),
		new CurvePoint(75f, 0.05f),
		new CurvePoint(100f, 0.5f),
		new CurvePoint(200f, 1f)
	};

	public static readonly SimpleCurve ChanceForUncommonFishFromWaterBodySizeCurve = new SimpleCurve
	{
		new CurvePoint(399.9f, 0f),
		new CurvePoint(400f, 0.1f),
		new CurvePoint(1600f, 0.25f),
		new CurvePoint(6000f, 0.7f)
	};

	public static readonly SimpleCurve PopulationToFishYieldCurve = new SimpleCurve
	{
		new CurvePoint(25f, 1f),
		new CurvePoint(100f, 3f),
		new CurvePoint(300f, 6f)
	};

	public static readonly SimpleCurve PollutionToxfishChanceCurve = new SimpleCurve
	{
		new CurvePoint(0.05f, 0f),
		new CurvePoint(0.8f, 0.6f)
	};

	private static List<NegativeFishingOutcomeDef> tmpNegativeCatches = new List<NegativeFishingOutcomeDef>();

	private static List<Thing> tmpCatches = new List<Thing>();

	public static string FishPopulationLabel(float population)
	{
		if (population < 50f)
		{
			return "FishPopulation_VeryLow".Translate();
		}
		if (population < 200f)
		{
			return "FishPopulation_Low".Translate();
		}
		if (population < 300f)
		{
			return "FishPopulation_Moderate".Translate();
		}
		return "FishPopulation_Healthy".Translate();
	}

	public static WaterBody GetWaterBody(this IntVec3 c, Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return null;
		}
		return map.waterBodyTracker.WaterBodyAt(c);
	}

	public static List<NegativeFishingOutcomeDef> GetNegativeFishingOutcomes(Pawn pawn, IntVec3 cell)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return null;
		}
		tmpNegativeCatches.Clear();
		if (!DebugSettings.alwaysNegativeCatches)
		{
			if (!Rand.Chance(0.02f))
			{
				return tmpNegativeCatches;
			}
			if (pawn.Map.waterBodyTracker.lastNegativeCatchTick != 0 && GenTicks.TicksGame - pawn.Map.waterBodyTracker.lastNegativeCatchTick <= 300000)
			{
				return tmpNegativeCatches;
			}
		}
		WaterBody waterBody = cell.GetWaterBody(pawn.Map);
		foreach (NegativeFishingOutcomeDef item in DefDatabase<NegativeFishingOutcomeDef>.AllDefsListForReading)
		{
			if (waterBody.CommonFish.Contains(item.fishType) || waterBody.UncommonFish.Contains(item.fishType))
			{
				tmpNegativeCatches.Add(item);
			}
		}
		return tmpNegativeCatches;
	}

	public static List<Thing> GetCatchesFor(Pawn pawn, IntVec3 cell, bool animalFishing, out bool rare)
	{
		tmpCatches.Clear();
		rare = false;
		if (!ModsConfig.OdysseyActive)
		{
			return tmpCatches;
		}
		WaterBody waterBody = cell.GetWaterBody(pawn.Map);
		if (waterBody == null || waterBody.waterBodyType == WaterBodyType.None)
		{
			return tmpCatches;
		}
		ThingDef def;
		if (Rand.Chance(PollutionToxfishChanceCurve.Evaluate(waterBody.PollutionPct)))
		{
			def = ThingDefOf.Fish_Toxfish;
			rare = false;
		}
		else
		{
			if (!animalFishing && pawn.Map.Biome.fishTypes.rareCatchesSetMaker != null && (DebugSettings.alwaysRareCatches || pawn.Map.waterBodyTracker.lastRareCatchTick == 0 || GenTicks.TicksGame - pawn.Map.waterBodyTracker.lastRareCatchTick > 300000) && Rand.Chance(0.01f))
			{
				tmpCatches.AddRange(pawn.Map.Biome.fishTypes.rareCatchesSetMaker.root.Generate());
				if (tmpCatches.Any())
				{
					rare = true;
					return tmpCatches;
				}
			}
			rare = false;
			if ((animalFishing || !Rand.Chance(0.05f) || !waterBody.UncommonFish.TryRandomElement(out var result)) && !waterBody.CommonFishIncludingExtras.TryRandomElement(out result))
			{
				return tmpCatches;
			}
			def = result;
		}
		float x = pawn.Map.waterBodyTracker.FishPopulationAt(cell);
		float num;
		if (animalFishing)
		{
			float statValueAbstract = def.GetStatValueAbstract(StatDefOf.Nutrition);
			num = Mathf.Ceil(FoodUtility.WillIngestStackCountOf(pawn, def, statValueAbstract));
		}
		else
		{
			num = Mathf.Min(PopulationToFishYieldCurve.Evaluate(x) * pawn.GetStatValue(StatDefOf.FishingYield));
		}
		float num2 = pawn.Map.waterBodyTracker.FishPopulationAt(cell);
		if (num > num2)
		{
			num = num2;
		}
		int stackCount = Mathf.Max(1, Mathf.RoundToInt(num));
		Thing thing = ThingMaker.MakeThing(def);
		thing.stackCount = stackCount;
		tmpCatches.Add(thing);
		return tmpCatches;
	}
}
