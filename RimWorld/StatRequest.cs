using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public struct StatRequest : IEquatable<StatRequest>
	{
		private Thing thingInt;

		private Def defInt;

		private ThingDef stuffDefInt;

		private QualityCategory qualityCategoryInt;

		private Faction faction;

		private Pawn pawn;

		public Thing Thing => thingInt;

		public Def Def => defInt;

		public BuildableDef BuildableDef => (BuildableDef)defInt;

		public AbilityDef AbilityDef => (AbilityDef)defInt;

		public Faction Faction => faction;

		public Pawn Pawn => pawn;

		public bool ForAbility => defInt is AbilityDef;

		public List<StatModifier> StatBases
		{
			get
			{
				if (!(defInt is BuildableDef))
				{
					return AbilityDef.statBases;
				}
				return BuildableDef.statBases;
			}
		}

		public ThingDef StuffDef => stuffDefInt;

		public QualityCategory QualityCategory => qualityCategoryInt;

		public bool HasThing => Thing != null;

		public bool Empty => Def == null;

		public static StatRequest For(Thing thing)
		{
			if (thing == null)
			{
				Log.Error("StatRequest for null thing.");
				return ForEmpty();
			}
			StatRequest result = default(StatRequest);
			result.thingInt = thing;
			result.defInt = thing.def;
			result.stuffDefInt = thing.Stuff;
			thing.TryGetQuality(out result.qualityCategoryInt);
			return result;
		}

		public static StatRequest For(Thing thing, Pawn pawn)
		{
			if (thing == null)
			{
				Log.Error("StatRequest for null thing.");
				return ForEmpty();
			}
			StatRequest result = default(StatRequest);
			result.thingInt = thing;
			result.defInt = thing.def;
			result.stuffDefInt = thing.Stuff;
			result.pawn = pawn;
			thing.TryGetQuality(out result.qualityCategoryInt);
			return result;
		}

		public static StatRequest For(BuildableDef def, ThingDef stuffDef, QualityCategory quality = QualityCategory.Normal)
		{
			if (def == null)
			{
				Log.Error("StatRequest for null def.");
				return ForEmpty();
			}
			StatRequest result = default(StatRequest);
			result.thingInt = null;
			result.defInt = def;
			result.stuffDefInt = stuffDef;
			result.qualityCategoryInt = quality;
			return result;
		}

		public static StatRequest For(AbilityDef def)
		{
			if (def == null)
			{
				Log.Error("StatRequest for null def.");
				return ForEmpty();
			}
			StatRequest result = default(StatRequest);
			result.thingInt = null;
			result.stuffDefInt = null;
			result.defInt = def;
			result.qualityCategoryInt = QualityCategory.Normal;
			return result;
		}

		public static StatRequest For(RoyalTitleDef def, Faction faction)
		{
			if (def == null)
			{
				Log.Error("StatRequest for null def.");
				return ForEmpty();
			}
			StatRequest result = default(StatRequest);
			result.thingInt = null;
			result.stuffDefInt = null;
			result.defInt = null;
			result.faction = faction;
			result.qualityCategoryInt = QualityCategory.Normal;
			return result;
		}

		public static StatRequest ForEmpty()
		{
			StatRequest result = default(StatRequest);
			result.thingInt = null;
			result.defInt = null;
			result.stuffDefInt = null;
			result.qualityCategoryInt = QualityCategory.Normal;
			return result;
		}

		public override string ToString()
		{
			if (Thing != null)
			{
				return string.Concat("(", Thing, ")");
			}
			return string.Concat("(", Thing, ", ", (StuffDef != null) ? StuffDef.defName : "null", ")");
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombineInt(seed, defInt.shortHash);
			if (thingInt != null)
			{
				seed = Gen.HashCombineInt(seed, thingInt.thingIDNumber);
			}
			if (stuffDefInt != null)
			{
				seed = Gen.HashCombineInt(seed, stuffDefInt.shortHash);
			}
			return seed;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is StatRequest))
			{
				return false;
			}
			StatRequest statRequest = (StatRequest)obj;
			if (statRequest.defInt == defInt && statRequest.thingInt == thingInt)
			{
				return statRequest.stuffDefInt == stuffDefInt;
			}
			return false;
		}

		public bool Equals(StatRequest other)
		{
			if (other.defInt == defInt && other.thingInt == thingInt)
			{
				return other.stuffDefInt == stuffDefInt;
			}
			return false;
		}

		public static bool operator ==(StatRequest lhs, StatRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(StatRequest lhs, StatRequest rhs)
		{
			return !(lhs == rhs);
		}
	}
}
