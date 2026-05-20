using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_TradeRequest_GetRequestedThing : QuestNode
{
	private static readonly IntRange BaseValueWantedRange = new IntRange(500, 2500);

	private static readonly SimpleCurve ValueWantedFactorFromWealthCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.3f),
		new CurvePoint(50000f, 1f),
		new CurvePoint(300000f, 2f)
	};

	private static Dictionary<ThingDef, int> requestCountDict = new Dictionary<ThingDef, int>();

	[NoTranslate]
	public SlateRef<string> storeThingAs;

	[NoTranslate]
	public SlateRef<string> storeThingCountAs;

	[NoTranslate]
	public SlateRef<string> storeMarketValueAs;

	[NoTranslate]
	public SlateRef<string> storeHasQualityAs;

	public SlateRef<List<ThingDef>> dontRequest;

	private static int RandomRequestCount(ThingDef thingDef, Map map)
	{
		Rand.PushState(Find.TickManager.TicksGame ^ thingDef.GetHashCode() ^ 0x343820DB);
		float num = BaseValueWantedRange.RandomInRange;
		Rand.PopState();
		num *= ValueWantedFactorFromWealthCurve.Evaluate(map.wealthWatcher.WealthTotal);
		return ThingUtility.RoundedResourceStackCount(Mathf.Max(1, Mathf.RoundToInt(num / thingDef.BaseMarketValue)));
	}

	private static bool TryFindRandomRequestedThingDef(Map map, out ThingDef thingDef, out int count, List<ThingDef> dontRequest)
	{
		requestCountDict.Clear();
		if (ThingSetMakerUtility.allGeneratableItems.Where(GlobalValidator).TryRandomElement(out thingDef))
		{
			count = requestCountDict[thingDef];
			return true;
		}
		count = 0;
		return false;
		bool GlobalValidator(ThingDef td)
		{
			if (td.BaseMarketValue / td.BaseMass < 5f)
			{
				return false;
			}
			if (!td.alwaysHaulable)
			{
				return false;
			}
			CompProperties_Rottable compProperties = td.GetCompProperties<CompProperties_Rottable>();
			if (compProperties != null && compProperties.daysToRotStart < 10f)
			{
				return false;
			}
			if (td.ingestible != null && td.ingestible.HumanEdible)
			{
				return false;
			}
			if (td == ThingDefOf.Silver)
			{
				return false;
			}
			if (!td.PlayerAcquirable)
			{
				return false;
			}
			int num = RandomRequestCount(td, map);
			requestCountDict.Add(td, num);
			if (!PlayerItemAccessibilityUtility.PossiblyAccessible(td, num, map))
			{
				return false;
			}
			if (!PlayerItemAccessibilityUtility.PlayerCanMake(td, map))
			{
				return false;
			}
			if (td.thingSetMakerTags != null && td.thingSetMakerTags.Contains("RewardStandardHighFreq"))
			{
				return false;
			}
			if (!dontRequest.NullOrEmpty() && dontRequest.Contains(td))
			{
				return false;
			}
			return true;
		}
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (TryFindRandomRequestedThingDef(slate.Get<Map>("map"), out var thingDef, out var count, dontRequest.GetValue(slate)))
		{
			slate.Set(storeThingAs.GetValue(slate), thingDef);
			slate.Set(storeThingCountAs.GetValue(slate), count);
			slate.Set(storeMarketValueAs.GetValue(slate), thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * (float)count);
			slate.Set(storeHasQualityAs.GetValue(slate), thingDef.HasComp(typeof(CompQuality)));
		}
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (TryFindRandomRequestedThingDef(slate.Get<Map>("map"), out var thingDef, out var count, dontRequest.GetValue(slate)))
		{
			slate.Set(storeThingAs.GetValue(slate), thingDef);
			slate.Set(storeThingCountAs.GetValue(slate), count);
			slate.Set(storeMarketValueAs.GetValue(slate), thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * (float)count);
			return true;
		}
		return false;
	}
}
