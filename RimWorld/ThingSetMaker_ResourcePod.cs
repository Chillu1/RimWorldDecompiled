using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_ResourcePod : ThingSetMaker
	{
		private const int MaxStacks = 7;

		private const float MaxMarketValue = 40f;

		private const float MinMoney = 150f;

		private const float MaxMoney = 600f;

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			ThingDef thingDef = RandomPodContentsDef();
			float num = Rand.Range(150f, 600f);
			do
			{
				Thing thing = ThingMaker.MakeThing(thingDef);
				int num2 = Rand.Range(20, 40);
				if (num2 > thing.def.stackLimit)
				{
					num2 = thing.def.stackLimit;
				}
				if ((float)num2 * thing.def.BaseMarketValue > num)
				{
					num2 = Mathf.FloorToInt(num / thing.def.BaseMarketValue);
				}
				if (num2 == 0)
				{
					num2 = 1;
				}
				thing.stackCount = num2;
				outThings.Add(thing);
				num -= (float)num2 * thingDef.BaseMarketValue;
			}
			while (outThings.Count < 7 && !(num <= thingDef.BaseMarketValue));
		}

		private static IEnumerable<ThingDef> PossiblePodContentsDefs()
		{
			return DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.category == ThingCategory.Item && d.tradeability.TraderCanSell() && d.equipmentType == EquipmentType.None && d.BaseMarketValue >= 1f && d.BaseMarketValue < 40f && !d.HasComp(typeof(CompHatcher)));
		}

		public static ThingDef RandomPodContentsDef(bool mustBeResource = false)
		{
			IEnumerable<ThingDef> source = PossiblePodContentsDefs();
			if (mustBeResource)
			{
				source = source.Where((ThingDef x) => x.stackLimit > 1);
			}
			int numMeats = source.Where((ThingDef x) => x.IsMeat).Count();
			int numLeathers = source.Where((ThingDef x) => x.IsLeather).Count();
			return source.RandomElementByWeight((ThingDef d) => ThingSetMakerUtility.AdjustedBigCategoriesSelectionWeight(d, numMeats, numLeathers));
		}

		[DebugOutput("Incidents", false)]
		private static void PodContentsPossibleDefs()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("ThingDefs that can go in the resource pod crash incident.");
			foreach (ThingDef item in PossiblePodContentsDefs())
			{
				stringBuilder.AppendLine(item.defName);
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput("Incidents", false)]
		private static void PodContentsTest()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < 100; i++)
			{
				stringBuilder.AppendLine(RandomPodContentsDef().LabelCap);
			}
			Log.Message(stringBuilder.ToString());
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			return PossiblePodContentsDefs();
		}
	}
}
