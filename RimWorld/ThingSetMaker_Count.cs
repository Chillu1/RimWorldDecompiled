using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ThingSetMaker_Count : ThingSetMaker
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
		if (parms.maxTotalMass.HasValue && parms.maxTotalMass != float.MaxValue && !ThingSetMakerUtility.PossibleToWeighNoMoreThan(AllowedThingDefs(parms), parms.techLevel.GetValueOrDefault(), parms.maxTotalMass.Value, parms.countRange?.max ?? 1))
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
		TechLevel valueOrDefault = parms.techLevel.GetValueOrDefault();
		IntRange intRange = parms.countRange ?? IntRange.One;
		float num = parms.maxTotalMass ?? float.MaxValue;
		int num2 = Mathf.Max(intRange.RandomInRange, 1);
		float num3 = 0f;
		for (int i = 0; i < num2; i++)
		{
			if (!ThingSetMakerUtility.TryGetRandomThingWhichCanWeighNoMoreThan(enumerable, valueOrDefault, (num == float.MaxValue) ? float.MaxValue : (num - num3), parms.qualityGenerator, out var thingStuffPair))
			{
				break;
			}
			Thing thing = ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
			ThingSetMakerUtility.AssignQuality(thing, parms.qualityGenerator);
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
		TechLevel techLevel = parms.techLevel.GetValueOrDefault();
		foreach (ThingDef item in AllowedThingDefs(parms))
		{
			if (!parms.maxTotalMass.HasValue || parms.maxTotalMass == float.MaxValue || !(ThingSetMakerUtility.GetMinMass(item, techLevel) > parms.maxTotalMass))
			{
				yield return item;
			}
		}
	}
}
