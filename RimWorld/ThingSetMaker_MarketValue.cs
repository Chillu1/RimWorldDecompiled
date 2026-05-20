using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class ThingSetMaker_MarketValue : ThingSetMaker
{
	private int nextSeed;

	public ThingSetMaker_MarketValue()
	{
		nextSeed = Rand.Int;
	}

	protected override bool CanGenerateSub(ThingSetMakerParams parms)
	{
		List<ThingDef> list = AllowedThingDefs(parms).ToList();
		if (!list.Any())
		{
			return false;
		}
		if (parms.countRange.HasValue && parms.countRange.Value.max <= 0)
		{
			return false;
		}
		if (!parms.totalMarketValueRange.HasValue || parms.totalMarketValueRange.Value.max <= 0f)
		{
			return false;
		}
		float totalMarketValue;
		if (parms.maxTotalMass.HasValue)
		{
			float? maxTotalMass = parms.maxTotalMass;
			totalMarketValue = float.MaxValue;
			if (maxTotalMass != totalMarketValue && !ThingSetMakerUtility.PossibleToWeighNoMoreThan(list, parms.techLevel.GetValueOrDefault(), parms.maxTotalMass.Value, (!parms.countRange.HasValue) ? 1 : parms.countRange.Value.min))
			{
				return false;
			}
		}
		if (!GeneratePossibleDefs(parms, out totalMarketValue, nextSeed).Any())
		{
			return false;
		}
		return true;
	}

	protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
	{
		float maxMass = parms.maxTotalMass ?? float.MaxValue;
		float totalMarketValue;
		List<ThingStuffPairWithQuality> list = GeneratePossibleDefs(parms, out totalMarketValue, nextSeed);
		for (int i = 0; i < list.Count; i++)
		{
			outThings.Add(list[i].MakeThing());
		}
		ThingSetMakerByTotalStatUtility.IncreaseStackCountsToTotalValue(outThings, totalMarketValue, (Thing x) => x.MarketValue, maxMass, satisfyMinRewardCount: true);
		nextSeed++;
	}

	protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
	{
		return ThingSetMakerUtility.GetAllowedThingDefs(parms);
	}

	private float GetSingleThingValue(ThingStuffPairWithQuality thingStuffPair)
	{
		return thingStuffPair.GetStatValue(StatDefOf.MarketValue);
	}

	private float GetMinValue(ThingStuffPairWithQuality thingStuffPair)
	{
		return thingStuffPair.GetStatValue(StatDefOf.MarketValue) * (float)thingStuffPair.thing.minRewardCount;
	}

	private float GetMaxValue(ThingStuffPairWithQuality thingStuffPair)
	{
		return thingStuffPair.GetStatValue(StatDefOf.MarketValue) * (float)thingStuffPair.thing.stackLimit;
	}

	private List<ThingStuffPairWithQuality> GeneratePossibleDefs(ThingSetMakerParams parms, out float totalMarketValue, int seed)
	{
		Rand.PushState(seed);
		List<ThingStuffPairWithQuality> result = GeneratePossibleDefs(parms, out totalMarketValue);
		Rand.PopState();
		return result;
	}

	private List<ThingStuffPairWithQuality> GeneratePossibleDefs(ThingSetMakerParams parms, out float totalMarketValue)
	{
		IEnumerable<ThingDef> enumerable = AllowedThingDefs(parms);
		if (!enumerable.Any())
		{
			totalMarketValue = 0f;
			return new List<ThingStuffPairWithQuality>();
		}
		TechLevel valueOrDefault = parms.techLevel.GetValueOrDefault();
		IntRange countRange = parms.countRange ?? new IntRange(1, int.MaxValue);
		FloatRange floatRange = parms.totalMarketValueRange ?? FloatRange.Zero;
		float maxMass = parms.maxTotalMass ?? float.MaxValue;
		QualityGenerator valueOrDefault2 = parms.qualityGenerator.GetValueOrDefault();
		totalMarketValue = floatRange.RandomInRange;
		return ThingSetMakerByTotalStatUtility.GenerateDefsWithPossibleTotalValue(countRange, totalMarketValue, enumerable, valueOrDefault, valueOrDefault2, GetMinValue, GetMaxValue, GetSingleThingValue, null, 100, maxMass, parms.allowNonStackableDuplicates ?? true, totalMarketValue * parms.minSingleItemMarketValuePct.GetValueOrDefault());
	}

	protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
	{
		TechLevel techLevel = parms.techLevel.GetValueOrDefault();
		foreach (ThingDef item in AllowedThingDefs(parms))
		{
			if ((!parms.maxTotalMass.HasValue || parms.maxTotalMass == float.MaxValue || !(ThingSetMakerUtility.GetMinMass(item, techLevel) > parms.maxTotalMass)) && (!parms.totalMarketValueRange.HasValue || parms.totalMarketValueRange.Value.max == float.MaxValue || !(ThingSetMakerUtility.GetMinMarketValue(item, techLevel) > parms.totalMarketValueRange.Value.max)))
			{
				yield return item;
			}
		}
	}
}
