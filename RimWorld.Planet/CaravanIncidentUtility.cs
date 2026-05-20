using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class CaravanIncidentUtility
{
	private const int MapCellsPerPawn = 900;

	private const int MinMapSize = 75;

	private const int MaxMapSize = 110;

	public static int CalculateIncidentMapSize(List<Pawn> caravanPawns, List<Pawn> enemies)
	{
		return Mathf.Clamp(Mathf.RoundToInt(Mathf.Sqrt(Mathf.RoundToInt((caravanPawns.Count + enemies.Count) * 900))), 75, 110);
	}

	public static bool CanFireIncidentWhichWantsToGenerateMapAt(PlanetTile tile)
	{
		if (Current.Game.FindMap(tile) != null)
		{
			return false;
		}
		if (!Find.WorldGrid[tile].PrimaryBiome.implemented)
		{
			return false;
		}
		List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
		for (int i = 0; i < allWorldObjects.Count; i++)
		{
			if (allWorldObjects[i].Tile == tile && !allWorldObjects[i].def.allowCaravanIncidentsWhichGenerateMap)
			{
				return false;
			}
		}
		return true;
	}

	public static Map SetupCaravanAttackMap(Caravan caravan, List<Pawn> enemies, bool sendLetterIfRelatedPawns)
	{
		int num = CalculateIncidentMapSize(caravan.PawnsListForReading, enemies);
		Map map = GetOrGenerateMapForIncident(caravan, new IntVec3(num, 1, num), WorldObjectDefOf.Ambush);
		MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out var playerStartingSpot, out var second);
		CaravanEnterMapUtility.Enter(caravan, map, (Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map), CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
		for (int num2 = 0; num2 < enemies.Count; num2++)
		{
			IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(second, map);
			GenSpawn.Spawn(enemies[num2], loc, map, Rot4.Random);
		}
		if (sendLetterIfRelatedPawns)
		{
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(enemies, "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
		}
		return map;
	}

	public static Map GetOrGenerateMapForIncident(Caravan caravan, IntVec3 size, WorldObjectDef suggestedMapParentDef)
	{
		PlanetTile tile = caravan.Tile;
		bool num = Current.Game.FindMap(tile) == null;
		Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, size, suggestedMapParentDef);
		if (num)
		{
			orGenerateMap?.retainedCaravanData.Notify_GeneratedTempIncidentMapFor(caravan);
		}
		return orGenerateMap;
	}
}
