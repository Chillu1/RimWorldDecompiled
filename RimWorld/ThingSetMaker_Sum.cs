using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ThingSetMaker_Sum : ThingSetMaker
{
	public class Option
	{
		public ThingSetMaker thingSetMaker;

		public float chance = 1f;

		public float? maxMarketValue;

		public float minMarketValue;

		public float minTotalMarketValue;

		public const float MaxMarketValueLeewayFactor = 0.8f;
	}

	public List<Option> options;

	public bool resolveInOrder;

	private List<Option> optionsInResolveOrder = new List<Option>();

	protected override bool CanGenerateSub(ThingSetMakerParams parms)
	{
		for (int i = 0; i < options.Count; i++)
		{
			if (options[i].chance > 0f && options[i].thingSetMaker.CanGenerate(parms))
			{
				return true;
			}
		}
		return false;
	}

	protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
	{
		int num = 0;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		optionsInResolveOrder.Clear();
		optionsInResolveOrder.AddRange(options);
		if (!resolveInOrder)
		{
			optionsInResolveOrder.Shuffle();
		}
		for (int i = 0; i < optionsInResolveOrder.Count; i++)
		{
			ThingSetMakerParams parms2 = parms;
			if (parms2.countRange.HasValue)
			{
				parms2.countRange = new IntRange(Mathf.Max(parms2.countRange.Value.min - num, 0), parms2.countRange.Value.max - num);
				if (parms2.countRange.Value.max <= 0)
				{
					continue;
				}
			}
			if (parms2.totalMarketValueRange.HasValue)
			{
				if (parms2.totalMarketValueRange.Value.max < optionsInResolveOrder[i].minTotalMarketValue)
				{
					continue;
				}
				float min = ((i < optionsInResolveOrder.Count - 1) ? 0f : Mathf.Max(parms2.totalMarketValueRange.Value.min - num2, 0f));
				float max = parms2.totalMarketValueRange.Value.max - num2;
				parms2.totalMarketValueRange = new FloatRange(min, max);
				if (optionsInResolveOrder[i].maxMarketValue.HasValue)
				{
					parms2.totalMarketValueRange = new FloatRange(Mathf.Min(parms2.totalMarketValueRange.Value.min, optionsInResolveOrder[i].maxMarketValue.Value * 0.8f), Mathf.Min(Mathf.Min(parms2.totalMarketValueRange.Value.max, optionsInResolveOrder[i].maxMarketValue.Value)));
				}
				if (parms2.totalMarketValueRange.Value.max <= 0f || parms2.totalMarketValueRange.Value.max < optionsInResolveOrder[i].minMarketValue)
				{
					continue;
				}
			}
			if (parms2.totalNutritionRange.HasValue)
			{
				parms2.totalNutritionRange = new FloatRange(Mathf.Max(parms2.totalNutritionRange.Value.min - num3, 0f), parms2.totalNutritionRange.Value.max - num3);
			}
			if (parms2.maxTotalMass.HasValue)
			{
				parms2.maxTotalMass -= num4;
			}
			if (!Rand.Chance(optionsInResolveOrder[i].chance) || !optionsInResolveOrder[i].thingSetMaker.CanGenerate(parms2))
			{
				continue;
			}
			List<Thing> list = optionsInResolveOrder[i].thingSetMaker.Generate(parms2);
			num += list.Count;
			for (int j = 0; j < list.Count; j++)
			{
				num2 += list[j].MarketValue * (float)list[j].stackCount;
				if (list[j].def.IsIngestible)
				{
					num3 += list[j].GetStatValue(StatDefOf.Nutrition) * (float)list[j].stackCount;
				}
				if (!(list[j] is Pawn))
				{
					num4 += list[j].GetStatValue(StatDefOf.Mass) * (float)list[j].stackCount;
				}
			}
			outThings.AddRange(list);
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		for (int i = 0; i < options.Count; i++)
		{
			options[i].thingSetMaker.ResolveReferences();
		}
	}

	protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
	{
		for (int i = 0; i < options.Count; i++)
		{
			if (options[i].chance <= 0f)
			{
				continue;
			}
			foreach (ThingDef item in options[i].thingSetMaker.AllGeneratableThingsDebug(parms))
			{
				yield return item;
			}
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (options.NullOrEmpty())
		{
			yield return "no options.";
			yield break;
		}
		for (int i = 0; i < options.Count; i++)
		{
			if (options[i].thingSetMaker == null)
			{
				yield return "null thingSetMaker";
				continue;
			}
			foreach (string item in options[i].thingSetMaker.ConfigErrors())
			{
				yield return item;
			}
		}
	}
}
