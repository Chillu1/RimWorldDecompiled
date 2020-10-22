using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public struct ThingStuffPair : IEquatable<ThingStuffPair>
	{
		public ThingDef thing;

		public ThingDef stuff;

		public float commonalityMultiplier;

		private float cachedPrice;

		private float cachedInsulationCold;

		private float cachedInsulationHeat;

		public float Price => cachedPrice;

		public float InsulationCold => cachedInsulationCold;

		public float InsulationHeat => cachedInsulationHeat;

		public float Commonality
		{
			get
			{
				float num = commonalityMultiplier;
				num *= thing.generateCommonality;
				if (stuff != null)
				{
					num *= stuff.stuffProps.commonality;
				}
				if (PawnWeaponGenerator.IsDerpWeapon(thing, stuff) || PawnApparelGenerator.IsDerpApparel(thing, stuff))
				{
					num *= 0.01f;
				}
				return num;
			}
		}

		public ThingStuffPair(ThingDef thing, ThingDef stuff, float commonalityMultiplier = 1f)
		{
			this.thing = thing;
			this.stuff = stuff;
			this.commonalityMultiplier = commonalityMultiplier;
			if (stuff != null && !thing.MadeFromStuff)
			{
				Log.Warning(string.Concat("Created ThingStuffPairWithQuality with stuff ", stuff, " but ", thing, " is not made from stuff."));
				stuff = null;
			}
			cachedPrice = thing.GetStatValueAbstract(StatDefOf.MarketValue, stuff);
			cachedInsulationCold = thing.GetStatValueAbstract(StatDefOf.Insulation_Cold, stuff);
			cachedInsulationHeat = thing.GetStatValueAbstract(StatDefOf.Insulation_Heat, stuff);
		}

		public static List<ThingStuffPair> AllWith(Predicate<ThingDef> thingValidator)
		{
			List<ThingStuffPair> list = new List<ThingStuffPair>();
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ThingDef thingDef = allDefsListForReading[i];
				if (!thingValidator(thingDef))
				{
					continue;
				}
				if (!thingDef.MadeFromStuff)
				{
					list.Add(new ThingStuffPair(thingDef, null));
					continue;
				}
				IEnumerable<ThingDef> enumerable = DefDatabase<ThingDef>.AllDefs.Where((ThingDef st) => st.IsStuff && st.stuffProps.CanMake(thingDef));
				int num = enumerable.Count();
				float num2 = enumerable.Average((ThingDef st) => st.stuffProps.commonality);
				float num3 = 1f / (float)num / num2;
				foreach (ThingDef item in enumerable)
				{
					list.Add(new ThingStuffPair(thingDef, item, num3));
				}
			}
			return list.OrderByDescending((ThingStuffPair p) => p.Price).ToList();
		}

		public override string ToString()
		{
			if (thing == null)
			{
				return "(null)";
			}
			string text = ((stuff != null) ? (thing.label + " " + stuff.LabelAsStuff) : thing.label);
			return text + " $" + Price.ToString("F0") + " c=" + Commonality.ToString("F4");
		}

		public static bool operator ==(ThingStuffPair a, ThingStuffPair b)
		{
			if (a.thing == b.thing && a.stuff == b.stuff)
			{
				return a.commonalityMultiplier == b.commonalityMultiplier;
			}
			return false;
		}

		public static bool operator !=(ThingStuffPair a, ThingStuffPair b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ThingStuffPair))
			{
				return false;
			}
			return Equals((ThingStuffPair)obj);
		}

		public bool Equals(ThingStuffPair other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineStruct(Gen.HashCombine(Gen.HashCombine(0, thing), stuff), commonalityMultiplier);
		}

		public static explicit operator ThingStuffPair(ThingStuffPairWithQuality p)
		{
			return new ThingStuffPair(p.thing, p.stuff);
		}
	}
}
