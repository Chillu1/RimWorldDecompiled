using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

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
			if (defInt is BuildableDef buildableDef)
			{
				return buildableDef.statBases;
			}
			if (defInt is AbilityDef abilityDef)
			{
				return abilityDef.statBases;
			}
			return null;
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
		StatRequest result = new StatRequest
		{
			thingInt = thing,
			defInt = thing.def,
			stuffDefInt = thing.Stuff
		};
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
		StatRequest result = new StatRequest
		{
			thingInt = thing,
			defInt = thing.def,
			stuffDefInt = thing.Stuff,
			pawn = pawn
		};
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
		return new StatRequest
		{
			thingInt = null,
			defInt = def,
			stuffDefInt = stuffDef,
			qualityCategoryInt = quality
		};
	}

	public static StatRequest For(AbilityDef def, Pawn forPawn = null)
	{
		if (def == null)
		{
			Log.Error("StatRequest for null def.");
			return ForEmpty();
		}
		return new StatRequest
		{
			thingInt = null,
			stuffDefInt = null,
			defInt = def,
			qualityCategoryInt = QualityCategory.Normal,
			pawn = forPawn
		};
	}

	public static StatRequest For(RoyalTitleDef def, Faction faction, Pawn pawn = null)
	{
		if (def == null)
		{
			Log.Error("StatRequest for null def.");
			return ForEmpty();
		}
		return new StatRequest
		{
			thingInt = null,
			stuffDefInt = null,
			defInt = null,
			faction = faction,
			qualityCategoryInt = QualityCategory.Normal,
			pawn = pawn
		};
	}

	public static StatRequest ForEmpty()
	{
		return new StatRequest
		{
			thingInt = null,
			defInt = null,
			stuffDefInt = null,
			qualityCategoryInt = QualityCategory.Normal
		};
	}

	public override string ToString()
	{
		if (Thing != null)
		{
			return "(" + Thing?.ToString() + ")";
		}
		return "(" + Thing?.ToString() + ", " + ((StuffDef != null) ? StuffDef.defName : "null") + ")";
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
		seed = Gen.HashCombineInt(seed, qualityCategoryInt.GetHashCode());
		if (faction != null)
		{
			seed = Gen.HashCombineInt(seed, faction.GetHashCode());
		}
		if (pawn != null)
		{
			seed = Gen.HashCombineInt(seed, pawn.GetHashCode());
		}
		return seed;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StatRequest statRequest))
		{
			return false;
		}
		if (statRequest.defInt == defInt && statRequest.thingInt == thingInt)
		{
			return statRequest.stuffDefInt == stuffDefInt;
		}
		return false;
	}

	public bool Equals(StatRequest other)
	{
		if (other.defInt == defInt && other.thingInt == thingInt && other.stuffDefInt == stuffDefInt && other.qualityCategoryInt == qualityCategoryInt && other.faction == faction)
		{
			return other.pawn == pawn;
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
