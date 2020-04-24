using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_StackCount : ThingSetMaker
	{
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
			if (parms.maxTotalMass.HasValue && parms.maxTotalMass != float.MaxValue && !ThingSetMakerUtility.PossibleToWeighNoMoreThan(AllowedThingDefs(parms), parms.techLevel ?? TechLevel.Undefined, parms.maxTotalMass.Value, (!parms.countRange.HasValue) ? 1 : parms.countRange.Value.max))
			{
				return false;
			}
			return true;
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			IEnumerable<ThingDef> enumerable = AllowedThingDefs(parms);
			if (!enumerable.Any())
			{
				return;
			}
			TechLevel stuffTechLevel = parms.techLevel ?? TechLevel.Undefined;
			IntRange intRange = parms.countRange ?? IntRange.one;
			float num = parms.maxTotalMass ?? float.MaxValue;
			int num2 = Mathf.Max(intRange.RandomInRange, 1);
			float num3 = 0f;
			int num4 = num2;
			ThingStuffPair thingStuffPair;
			while (num4 > 0 && ThingSetMakerUtility.TryGetRandomThingWhichCanWeighNoMoreThan(enumerable, stuffTechLevel, (num == float.MaxValue) ? float.MaxValue : (num - num3), parms.qualityGenerator, out thingStuffPair))
			{
				Thing thing = ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
				ThingSetMakerUtility.AssignQuality(thing, parms.qualityGenerator);
				int num5 = num4;
				if (num != float.MaxValue && !(thing is Pawn))
				{
					num5 = Mathf.Min(num5, Mathf.FloorToInt((num - num3) / thing.GetStatValue(StatDefOf.Mass)));
				}
				num4 -= (thing.stackCount = Mathf.Clamp(num5, 1, thing.def.stackLimit));
				outThings.Add(thing);
				if (!(thing is Pawn))
				{
					num3 += thing.GetStatValue(StatDefOf.Mass) * (float)thing.stackCount;
				}
			}
		}

		protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
		{
			return ThingSetMakerUtility.GetAllowedThingDefs(parms);
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			TechLevel techLevel = parms.techLevel ?? TechLevel.Undefined;
			foreach (ThingDef item in AllowedThingDefs(parms))
			{
				if (!parms.maxTotalMass.HasValue || parms.maxTotalMass == float.MaxValue || !(ThingSetMakerUtility.GetMinMass(item, techLevel) > parms.maxTotalMass))
				{
					yield return item;
				}
			}
		}
	}
}
