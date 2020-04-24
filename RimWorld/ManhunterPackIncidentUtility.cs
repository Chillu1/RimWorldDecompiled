using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ManhunterPackIncidentUtility
	{
		public const int MinAnimalCount = 2;

		public const float MinPoints = 70f;

		public static float ManhunterAnimalWeight(PawnKindDef animal, float points)
		{
			points = Mathf.Max(points, 70f);
			if (animal.combatPower * 2f > points)
			{
				return 0f;
			}
			int num = Mathf.RoundToInt(points / animal.combatPower);
			return Mathf.Clamp01(Mathf.InverseLerp(30f, 10f, num) + 0.001f);
		}

		public static bool TryFindManhunterAnimalKind(float points, int tile, out PawnKindDef animalKind)
		{
			return DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => k.RaceProps.Animal && k.canArriveManhunter && (tile == -1 || Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race))).TryRandomElementByWeight((PawnKindDef k) => ManhunterAnimalWeight(k, points), out animalKind);
		}

		public static int GetAnimalsCount(PawnKindDef animalKind, float points)
		{
			return Mathf.Max(Mathf.RoundToInt(points / animalKind.combatPower), 2);
		}

		[Obsolete("Obsolete, only used to avoid error when patching")]
		public static List<Pawn> GenerateAnimals(PawnKindDef animalKind, int tile, float points)
		{
			return GenerateAnimals_NewTmp(animalKind, tile, points);
		}

		public static List<Pawn> GenerateAnimals_NewTmp(PawnKindDef animalKind, int tile, float points, int animalCount = 0)
		{
			List<Pawn> list = new List<Pawn>();
			int num = (animalCount > 0) ? animalCount : GetAnimalsCount(animalKind, points);
			for (int i = 0; i < num; i++)
			{
				Pawn item = PawnGenerator.GeneratePawn(new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, tile));
				list.Add(item);
			}
			return list;
		}

		[DebugOutput]
		public static void ManhunterResults()
		{
			List<PawnKindDef> candidates = (from k in DefDatabase<PawnKindDef>.AllDefs
				where k.RaceProps.Animal && k.canArriveManhunter
				orderby 0f - k.combatPower
				select k).ToList();
			List<float> list = new List<float>();
			for (int i = 0; i < 30; i++)
			{
				list.Add(20f * Mathf.Pow(1.25f, i));
			}
			DebugTables.MakeTablesDialog(list, (float points) => points.ToString("F0") + " pts", candidates, (PawnKindDef candidate) => candidate.defName + " (" + candidate.combatPower.ToString("F0") + ")", delegate(float points, PawnKindDef candidate)
			{
				float num = candidates.Sum((PawnKindDef k) => ManhunterAnimalWeight(k, points));
				float num2 = ManhunterAnimalWeight(candidate, points);
				return (num2 == 0f) ? "0%" : string.Format("{0}%, {1}", (num2 * 100f / num).ToString("F0"), Mathf.Max(Mathf.RoundToInt(points / candidate.combatPower), 1));
			});
		}
	}
}
