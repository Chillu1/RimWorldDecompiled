using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_DesiccatedCorpses : SymbolResolver
{
	private static FloatRange DefaultCorpseDensity = new FloatRange(0.001f, 0.002f);

	private static IntRange DefaultCorpseAge = new IntRange(180000000, 720000000);

	private const float ChanceToFillRoom = 0.04f;

	private const int MaxRoomSizeToFill = 1000;

	private static readonly SimpleCurve CorpseSpawnChanceOverRoomSizeCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.25f),
		new CurvePoint(100f, 0.25f),
		new CurvePoint(400f, 0.15f)
	};

	private static HashSet<Room> tmpSeenRooms = new HashSet<Room>();

	public override void Resolve(ResolveParams rp)
	{
		if (rp.desiccatedCorpsePawnKind != null && !rp.desiccatedCorpsePawnKind.RaceProps.IsFlesh)
		{
			Log.Error("Cannot create desiccated corpses for non-flesh based pawns.");
			return;
		}
		Map map = BaseGen.globalSettings.map;
		IntRange? desiccatedCorpseRandomAgeRange = rp.desiccatedCorpseRandomAgeRange;
		if (!desiccatedCorpseRandomAgeRange.HasValue)
		{
			_ = DefaultCorpseAge;
		}
		else
		{
			desiccatedCorpseRandomAgeRange.GetValueOrDefault();
		}
		tmpSeenRooms.Clear();
		foreach (IntVec3 item in rp.rect)
		{
			Room room = item.GetRoom(map);
			if (room != null && !tmpSeenRooms.Contains(room) && !room.PsychologicallyOutdoors && room.CellCount < 1000 && Rand.Chance(0.04f))
			{
				int cellCount = room.CellCount;
				float chance = CorpseSpawnChanceOverRoomSizeCurve.Evaluate(cellCount);
				foreach (IntVec3 cell in room.Cells)
				{
					if (Rand.Chance(chance) && CanSpawnAt(cell, map))
					{
						PawnKindDef pawnKindDef = rp.desiccatedCorpsePawnKind ?? GetRandomPawnKindForCorpse();
						SpawnCorpse(pawnKindDef, cell, DefaultCorpseAge.RandomInRange, map);
					}
				}
			}
			tmpSeenRooms.Add(room);
		}
		tmpSeenRooms.Clear();
		int num = Mathf.Max(1, Mathf.RoundToInt((rp.dessicatedCorpseDensityRange ?? DefaultCorpseDensity).RandomInRange * (float)rp.rect.Area));
		for (int i = 0; i < num; i++)
		{
			IntVec3 spawnPosition = FindRandomCorpseSpawnPosition(rp.rect, map);
			if (spawnPosition.IsValid)
			{
				PawnKindDef pawnKindDef2 = rp.desiccatedCorpsePawnKind ?? GetRandomPawnKindForCorpse();
				SpawnCorpse(pawnKindDef2, spawnPosition, DefaultCorpseAge.RandomInRange, map);
			}
		}
	}

	private void SpawnCorpse(PawnKindDef pawnKindDef, IntVec3 spawnPosition, int age, Map map)
	{
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: true, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, 0f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: false, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 0f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: true, forceNoBackstory: true, forbidAnyTitle: true));
		if (!pawn.Dead)
		{
			pawn.Kill(null, null);
		}
		if (pawn.inventory != null)
		{
			pawn.inventory.DestroyAll();
		}
		if (pawn.apparel != null)
		{
			pawn.apparel.DestroyAll();
		}
		if (pawn.equipment != null)
		{
			pawn.equipment.DestroyAllEquipment();
		}
		pawn.Corpse.Age = age + Rand.Range(0, 900000);
		pawn.relations.hidePawnRelations = true;
		GenSpawn.Spawn(pawn.Corpse, spawnPosition, map);
		pawn.Corpse.GetComp<CompRottable>().RotProgress += pawn.Corpse.Age;
	}

	private PawnKindDef GetRandomPawnKindForCorpse()
	{
		return DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef pk) => pk.RaceProps.IsFlesh && pk.canBeScattered).RandomElement();
	}

	private static IntVec3 FindRandomCorpseSpawnPosition(CellRect rect, Map map)
	{
		foreach (IntVec3 item in rect.Cells.InRandomOrder())
		{
			if (CanSpawnAt(item, map))
			{
				return item;
			}
		}
		return IntVec3.Invalid;
	}

	private static bool CanSpawnAt(IntVec3 cell, Map map)
	{
		if (!cell.Impassable(map))
		{
			return cell.GetThingList(map).Count == 0;
		}
		return false;
	}
}
