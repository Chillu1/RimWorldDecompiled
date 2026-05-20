using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class AggressiveAnimalIncidentUtility
{
	public const int MinAnimalCount = 2;

	public const int MaxAnimalCount = 100;

	public const float MinPoints = 70f;

	public static float AnimalWeight(PawnKindDef animal, float points)
	{
		points = Mathf.Max(points, 70f);
		if (animal.combatPower * 2f > points)
		{
			return 0f;
		}
		int num = Mathf.Min(Mathf.RoundToInt(points / animal.combatPower), 100);
		return Mathf.Clamp01(Mathf.InverseLerp(100f, 10f, num));
	}

	public static bool TryFindAggressiveAnimalKind(float points, PlanetTile tile, out PawnKindDef animalKind)
	{
		List<PawnKindDef> animals = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => CanArriveManhunter(k) && (!tile.Valid || Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race))).ToList();
		if (TryGetHabitatAnimal(tile, out animalKind))
		{
			return true;
		}
		return TryGetAnimalFromList(points, animals, out animalKind);
	}

	public static bool TryFindAggressiveAnimalKind(float points, Map map, out PawnKindDef animalKind)
	{
		if (TryGetHabitatAnimal(map.Tile, out animalKind))
		{
			return true;
		}
		bool polluted = ModsConfig.BiotechActive && map.Tile.Valid && Rand.Value < WildAnimalSpawner.PollutionAnimalSpawnChanceFromPollutionCurve.Evaluate(Find.WorldGrid[map.Tile].pollution);
		List<PawnKindDef> list = (from k in map.Biomes.SelectMany((BiomeDef b) => b.AllWildAnimals)
			where CanArriveWithPollution(k, map, polluted) && CanArriveManhunter(k)
			select k).ToList();
		if (polluted && list.Count == 0)
		{
			polluted = false;
			list = (from k in map.Biomes.SelectMany((BiomeDef b) => b.AllWildAnimals)
				where CanArriveWithPollution(k, map, polluted) && CanArriveManhunter(k)
				select k).ToList();
		}
		if (TryGetAnimalFromList(points, list, out animalKind))
		{
			return true;
		}
		list = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => CanArriveManhunter(k) && CanArriveWithPollution(k, map, polluted) && (!map.Tile.Valid || Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(map.Tile, k.race))).ToList();
		return TryGetAnimalFromList(points, list, out animalKind);
	}

	private static bool TryGetAnimalFromList(float points, List<PawnKindDef> animals, out PawnKindDef animalKind)
	{
		if (animals.Any())
		{
			if (animals.TryRandomElementByWeight((PawnKindDef a) => AnimalWeight(a, points), out animalKind))
			{
				return true;
			}
			if (points > animals.Min((PawnKindDef a) => a.combatPower) * 2f)
			{
				animalKind = animals.MaxBy((PawnKindDef a) => a.combatPower);
				return true;
			}
		}
		animalKind = null;
		return false;
	}

	private static bool TryGetHabitatAnimal(PlanetTile tile, out PawnKindDef animalKind)
	{
		animalKind = null;
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		if (!tile.Valid)
		{
			return false;
		}
		if (Find.WorldGrid[tile].Mutators.Contains(TileMutatorDefOf.AnimalHabitat))
		{
			TileMutatorWorker_AnimalHabitat tileMutatorWorker_AnimalHabitat = (TileMutatorWorker_AnimalHabitat)TileMutatorDefOf.AnimalHabitat.Worker;
			animalKind = tileMutatorWorker_AnimalHabitat.GetAnimalKind(tile);
			if (CanArriveManhunter(animalKind) && Rand.Chance(0.5f))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetAnimalsCount(PawnKindDef animalKind, float points)
	{
		return Mathf.Clamp(Mathf.RoundToInt(points / animalKind.combatPower), 2, 100);
	}

	public static List<Pawn> GenerateAnimals(PawnKindDef animalKind, PlanetTile tile, float points, int animalCount = 0)
	{
		List<Pawn> list = new List<Pawn>();
		int num = ((animalCount > 0) ? animalCount : GetAnimalsCount(animalKind, points));
		for (int i = 0; i < num; i++)
		{
			Pawn item = PawnGenerator.GeneratePawn(new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, tile));
			list.Add(item);
		}
		return list;
	}

	public static List<Pawn> GenerateAnimals(List<PawnKindDef> animalKinds, PlanetTile tile)
	{
		List<Pawn> list = new List<Pawn>();
		foreach (PawnKindDef animalKind in animalKinds)
		{
			Pawn item = PawnGenerator.GeneratePawn(new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, tile));
			list.Add(item);
		}
		return list;
	}

	[DebugOutput]
	public static void ManhunterResults()
	{
		List<PawnKindDef> candidates = (from k in DefDatabase<PawnKindDef>.AllDefs.Where(CanArriveManhunter)
			orderby 0f - k.combatPower
			select k).ToList();
		List<float> list = new List<float>();
		for (int num = 0; num < 30; num++)
		{
			list.Add(20f * Mathf.Pow(1.25f, num));
		}
		DebugTables.MakeTablesDialog(list, (float points) => points.ToString("F0") + " pts", candidates, (PawnKindDef candidate) => candidate.defName + " (" + candidate.combatPower.ToString("F0") + ")", delegate(float points, PawnKindDef candidate)
		{
			float num2 = candidates.Sum((PawnKindDef k) => AnimalWeight(k, points));
			float num3 = AnimalWeight(candidate, points);
			return (num3 != 0f) ? ((num3 * 100f / num2).ToString("F0") + $"%, {Mathf.Max(Mathf.RoundToInt(points / candidate.combatPower), 1)}") : "0%";
		});
	}

	private static bool CanArriveManhunter(PawnKindDef kind)
	{
		if (kind.RaceProps.Animal && kind.canArriveManhunter)
		{
			return kind.RaceProps.CanPassFences;
		}
		return false;
	}

	private static bool CanArriveWithPollution(PawnKindDef k, Map map, bool polluted)
	{
		if (polluted)
		{
			return map.Biomes.Any((BiomeDef b) => b.CommonalityOfPollutionAnimal(k) > 0f);
		}
		if (map.TileInfo.IsCoastal)
		{
			return map.Biomes.Any((BiomeDef b) => b.CommonalityOfAnimal(k) > 0f || b.CommonalityOfCoastalAnimal(k) > 0f);
		}
		return map.Biomes.Any((BiomeDef b) => b.CommonalityOfAnimal(k) > 0f);
	}
}
