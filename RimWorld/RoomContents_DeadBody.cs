using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class RoomContents_DeadBody : RoomContentsWorker
{
	private int? setAgeTicks;

	protected virtual FloatRange CorpseAgeDaysRange => new FloatRange(3f, 30f);

	protected virtual IntRange SurvivalPacksCountRange => new IntRange(0, 2);

	protected virtual IntRange BloodFilthRange => new IntRange(1, 5);

	protected virtual IntRange CorpseRange => new IntRange(1, 1);

	protected virtual bool AllHaveSameDeathAge => false;

	protected abstract ThingDef KillerThing { get; }

	protected abstract DamageDef DamageType { get; }

	protected abstract Tool ToolUsed { get; }

	protected virtual IEnumerable<PawnKindDef> GetPossibleKinds()
	{
		yield return PawnKindDefOf.Pirate;
		yield return PawnKindDefOf.Villager;
		yield return PawnKindDefOf.PirateBoss;
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		SpawnCorpses(map, room);
		base.FillRoom(map, room, faction, threatPoints);
	}

	protected virtual void SpawnCorpses(Map map, LayoutRoom room)
	{
		int randomInRange = CorpseRange.RandomInRange;
		if (AllHaveSameDeathAge)
		{
			setAgeTicks = Mathf.RoundToInt(CorpseAgeDaysRange.RandomInRange * 60000f);
		}
		else
		{
			setAgeTicks = null;
		}
		for (int i = 0; i < randomInRange; i++)
		{
			Corpse corpse = SpawnCorpse(map, room);
			int randomInRange2 = SurvivalPacksCountRange.RandomInRange;
			for (int j = 0; j < randomInRange2; j++)
			{
				GenDrop.TryDropSpawn(ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack), corpse.Position, map, ThingPlaceMode.Near, out var _);
			}
		}
	}

	protected Corpse SpawnCorpse(Map map, LayoutRoom room)
	{
		if (!room.TryGetRandomCellInRoom(map, out var cell, 1))
		{
			return null;
		}
		return SpawnCorpse(cell, map);
	}

	protected Corpse SpawnCorpse(IntVec3 cell, Map map)
	{
		PawnKindDef kind = GetPossibleKinds().RandomElement();
		int deadTicks = setAgeTicks ?? Mathf.RoundToInt(CorpseAgeDaysRange.RandomInRange * 60000f);
		return SpawnCorpse(cell, kind, deadTicks, map);
	}

	protected Corpse SpawnCorpse(IntVec3 cell, PawnKindDef kind, int deadTicks, Map map, float? fixedAge = null, bool forceNoGear = false)
	{
		return RoomGenUtility.SpawnCorpse(cell, kind, deadTicks, map, DamageType, fixedAge, forceNoGear, KillerThing, ToolUsed, BloodFilthRange);
	}
}
