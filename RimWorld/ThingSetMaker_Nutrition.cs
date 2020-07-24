using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Nutrition : ThingSetMaker
	{
		private int nextSeed;

		public ThingSetMaker_Nutrition()
		{
			nextSeed = Rand.Int;
		}

		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			if (!AllowedThingDefs(parms).Any())
			{
				return false;
			}
			if (parms.countRange.HasValue && parms.countRange.Value.max <= 0)
			{
				return false;
			}
			if (!parms.totalNutritionRange.HasValue || parms.totalNutritionRange.Value.max <= 0f)
			{
				return false;
			}
			float totalNutrition;
			if (parms.maxTotalMass.HasValue)
			{
				float? maxTotalMass = parms.maxTotalMass;
				totalNutrition = float.MaxValue;
				if (!(maxTotalMass == totalNutrition) && !ThingSetMakerUtility.PossibleToWeighNoMoreThan(AllowedThingDefs(parms), parms.techLevel ?? TechLevel.Undefined, parms.maxTotalMass.Value, (!parms.countRange.HasValue) ? 1 : parms.countRange.Value.min))
				{
					return false;
				}
			}
			if (!GeneratePossibleDefs(parms, out totalNutrition, nextSeed).Any())
			{
				return false;
			}
			return true;
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			float maxMass = parms.maxTotalMass ?? float.MaxValue;
			float totalNutrition;
			List<ThingStuffPairWithQuality> list = GeneratePossibleDefs(parms, out totalNutrition, nextSeed);
			for (int i = 0; i < list.Count; i++)
			{
				outThings.Add(list[i].MakeThing());
			}
			ThingSetMakerByTotalStatUtility.IncreaseStackCountsToTotalValue_NewTemp(outThings, totalNutrition, (Thing x) => x.GetStatValue(StatDefOf.Nutrition), maxMass);
			nextSeed++;
		}

		protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
		{
			return ThingSetMakerUtility.GetAllowedThingDefs(parms);
		}

		private List<ThingStuffPairWithQuality> GeneratePossibleDefs(ThingSetMakerParams parms, out float totalNutrition, int seed)
		{
			Rand.PushState(seed);
			List<ThingStuffPairWithQuality> result = GeneratePossibleDefs(parms, out totalNutrition);
			Rand.PopState();
			return result;
		}

		private List<ThingStuffPairWithQuality> GeneratePossibleDefs(ThingSetMakerParams parms, out float totalNutrition)
		{
			IEnumerable<ThingDef> enumerable = AllowedThingDefs(parms);
			if (!enumerable.Any())
			{
				totalNutrition = 0f;
				return new List<ThingStuffPairWithQuality>();
			}
			IntRange countRange = parms.countRange ?? new IntRange(1, int.MaxValue);
			FloatRange floatRange = parms.totalNutritionRange ?? FloatRange.Zero;
			TechLevel techLevel = parms.techLevel ?? TechLevel.Undefined;
			float maxMass = parms.maxTotalMass ?? float.MaxValue;
			QualityGenerator qualityGenerator = parms.qualityGenerator ?? QualityGenerator.BaseGen;
			totalNutrition = floatRange.RandomInRange;
			int numMeats = enumerable.Count((ThingDef x) => x.IsMeat);
			int numLeathers = enumerable.Count((ThingDef x) => x.IsLeather);
			return ThingSetMakerByTotalStatUtility.GenerateDefsWithPossibleTotalValue_NewTmp3(weightSelector: (ThingDef x) => ThingSetMakerUtility.AdjustedBigCategoriesSelectionWeight(x, numMeats, numLeathers), countRange: countRange, totalValue: totalNutrition, allowed: enumerable, techLevel: techLevel, qualityGenerator: qualityGenerator, getMinValue: (ThingStuffPairWithQuality x) => x.GetStatValue(StatDefOf.Nutrition), getMaxValue: (ThingStuffPairWithQuality x) => x.GetStatValue(StatDefOf.Nutrition) * (float)x.thing.stackLimit, getSingleThingValue: (ThingStuffPairWithQuality x) => x.GetStatValue(StatDefOf.Nutrition), tries: 100, maxMass: maxMass);
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			TechLevel techLevel = parms.techLevel ?? TechLevel.Undefined;
			foreach (ThingDef item in AllowedThingDefs(parms))
			{
				if ((!parms.maxTotalMass.HasValue || parms.maxTotalMass == float.MaxValue || !(ThingSetMakerUtility.GetMinMass(item, techLevel) > parms.maxTotalMass)) && (!parms.totalNutritionRange.HasValue || parms.totalNutritionRange.Value.max == float.MaxValue || !item.IsNutritionGivingIngestible || !(item.ingestible.CachedNutrition > parms.totalNutritionRange.Value.max)))
				{
					yield return item;
				}
			}
		}
	}
}
