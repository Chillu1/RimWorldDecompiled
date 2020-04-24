using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_RandomOption : ThingSetMaker
	{
		public class Option
		{
			public ThingSetMaker thingSetMaker;

			public float weight;

			public float? weightIfPlayerHasNoItem;

			public ThingDef weightIfPlayerHasNoItemItem;
		}

		public List<Option> options;

		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			for (int i = 0; i < options.Count; i++)
			{
				if (options[i].thingSetMaker.CanGenerate(parms) && GetSelectionWeight(options[i], parms) > 0f)
				{
					return true;
				}
			}
			return false;
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			if (options.Where((Option x) => x.thingSetMaker.CanGenerate(parms)).TryRandomElementByWeight((Option x) => GetSelectionWeight(x, parms), out Option result))
			{
				outThings.AddRange(result.thingSetMaker.Generate(parms));
			}
		}

		private float GetSelectionWeight(Option option, ThingSetMakerParams parms)
		{
			if (option.weightIfPlayerHasNoItem.HasValue && !PlayerItemAccessibilityUtility.PlayerOrQuestRewardHas(option.weightIfPlayerHasNoItemItem))
			{
				return option.weightIfPlayerHasNoItem.Value * option.thingSetMaker.ExtraSelectionWeightFactor(parms);
			}
			return option.weight * option.thingSetMaker.ExtraSelectionWeightFactor(parms);
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
				float num = options[i].weight;
				if (options[i].weightIfPlayerHasNoItem.HasValue)
				{
					num = Mathf.Max(num, options[i].weightIfPlayerHasNoItem.Value);
				}
				if (!(num <= 0f))
				{
					foreach (ThingDef item in options[i].thingSetMaker.AllGeneratableThingsDebug(parms))
					{
						yield return item;
					}
				}
			}
		}
	}
}
