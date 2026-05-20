using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ThingSetMaker_UniqueWeapon : ThingSetMaker
{
	protected override bool CanGenerateSub(ThingSetMakerParams parms)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		if (parms.countRange.HasValue && parms.countRange.Value.max <= 0)
		{
			return false;
		}
		if (parms.totalMarketValueRange.HasValue && parms.totalMarketValueRange.Value.max <= 0f)
		{
			return false;
		}
		return AllGeneratableThingsDebugSub(parms).Any();
	}

	protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
	{
		int num = parms.countRange?.RandomInRange ?? 1;
		if (parms.countRange.HasValue)
		{
			num = Mathf.Max(parms.countRange.Value.RandomInRange, num);
		}
		FloatRange floatRange = parms.totalMarketValueRange ?? new FloatRange(0f, float.MaxValue);
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			bool flag = i == num - 1;
			int num3 = 999;
			Thing thing;
			do
			{
				thing = ThingMaker.MakeThing(DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.HasComp<CompUniqueWeapon>()).RandomElement());
			}
			while (num3-- > 0 && (num2 + thing.MarketValue > floatRange.max || (flag && num2 + thing.MarketValue < floatRange.min)));
			if (num3 > 0)
			{
				num2 += thing.MarketValue;
				outThings.Add(thing);
				continue;
			}
			break;
		}
	}

	protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
	{
		return DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.HasComp<CompUniqueWeapon>());
	}
}
