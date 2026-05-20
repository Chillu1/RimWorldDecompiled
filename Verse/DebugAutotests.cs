using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public static class DebugAutotests
{
	private static List<PawnKindDef> pawnKindsForDamageTypeBattleRoyale;

	[DebugAction("Autotests", "Make colony (full)", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void MakeColonyFull()
	{
		Autotests_ColonyMaker.MakeColony_Full();
	}

	[DebugAction("Autotests", "Make colony (animals)", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void MakeColonyAnimals()
	{
		Autotests_ColonyMaker.MakeColony_Animals();
	}

	[DebugAction("Autotests", "Make colony (ancient junk)", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void MakeColonyAncientJunk()
	{
		Autotests_ColonyMaker.MakeColony_AncientJunk();
	}

	[DebugAction("Autotests", "Test force downed x100", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void TestForceDownedx100()
	{
		for (int i = 0; i < 100; i++)
		{
			PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
			Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionDef));
			GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.CurrentMap), Find.CurrentMap), Find.CurrentMap);
			HealthUtility.DamageUntilDowned(pawn);
			if (pawn.Dead)
			{
				Log.Error("Pawn died while force downing: " + pawn?.ToString() + " at " + pawn.Position.ToString());
				break;
			}
		}
	}

	[DebugAction("Autotests", "Test force kill x100", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void TestForceKillx100()
	{
		for (int i = 0; i < 100; i++)
		{
			PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
			Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionDef));
			GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.CurrentMap), Find.CurrentMap), Find.CurrentMap);
			HealthUtility.DamageUntilDead(pawn);
			if (!pawn.Dead)
			{
				Log.Error("Pawn died not die: " + pawn?.ToString() + " at " + pawn.Position.ToString());
				break;
			}
		}
	}

	[DebugAction("Autotests", "Test generate pawn x1000", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void TestGeneratePawnx1000()
	{
		float[] array = new float[10] { 10f, 20f, 50f, 100f, 200f, 500f, 1000f, 2000f, 5000f, 1E+20f };
		int[] array2 = new int[array.Length];
		for (int i = 0; i < 1000; i++)
		{
			PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
			PerfLogger.Reset();
			Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionDef));
			float ms = PerfLogger.Duration() * 1000f;
			array2[array.FirstIndexOf((float time) => ms <= time)]++;
			if (pawn.Dead)
			{
				Log.Error("Pawn is dead");
			}
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Pawn creation time histogram:");
		for (int num = 0; num < array2.Length; num++)
		{
			stringBuilder.AppendLine($"<{array[num]}ms: {array2[num]}");
		}
		Log.Message(stringBuilder.ToString());
	}

	[DebugAction("Autotests", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void GeneratePawnsOfAllShapes()
	{
		Rot4[] array = new Rot4[4]
		{
			Rot4.North,
			Rot4.East,
			Rot4.South,
			Rot4.West
		};
		IntVec3 intVec = UI.MouseCell();
		foreach (BodyTypeDef allDef in DefDatabase<BodyTypeDef>.AllDefs)
		{
			IntVec3 intVec2 = intVec;
			Rot4[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Rot4 rot = array2[i];
				PawnGenerationRequest request = new PawnGenerationRequest(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
				request.ForceBodyType = allDef;
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				string text = allDef.defName + "-" + rot.ToStringWord();
				pawn.Name = new NameTriple(text, text, text);
				GenSpawn.Spawn(pawn, intVec2, Find.CurrentMap);
				pawn.apparel.DestroyAll();
				pawn.drafter.Drafted = true;
				pawn.stances.SetStance(new Stance_Warmup(100000, intVec2 + rot.FacingCell, null));
				intVec2 += IntVec3.South * 2;
			}
			intVec += IntVec3.East * 2;
		}
	}

	[DebugAction("Autotests", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void CheckRegionListers()
	{
		Autotests_RegionListers.CheckBugs(Find.CurrentMap);
	}

	[DebugAction("Autotests", "Test time-to-down", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void TestTimeToDown()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (PawnKindDef kindDef in DefDatabase<PawnKindDef>.AllDefs.OrderBy((PawnKindDef kd) => kd.defName))
		{
			list.Add(new DebugMenuOption(kindDef.label, DebugMenuOptionMode.Action, delegate
			{
				if (kindDef == PawnKindDefOf.Colonist)
				{
					Log.Message("Current colonist TTD reference point: 22.3 seconds, stddev 8.35 seconds");
				}
				List<float> results = new List<float>();
				List<PawnKindDef> list2 = new List<PawnKindDef>();
				List<PawnKindDef> list3 = new List<PawnKindDef>();
				list2.Add(kindDef);
				list3.Add(kindDef);
				ArenaUtility.BeginArenaFightSet(1000, list2, list3, delegate(ArenaUtility.ArenaResult result)
				{
					if (result.winner != ArenaUtility.ArenaResult.Winner.Other)
					{
						results.Add(result.tickDuration.TicksToSeconds());
					}
				}, delegate
				{
					Log.Message($"Finished {results.Count} tests; time-to-down {results.Average()}, stddev {GenMath.Stddev(results)}\n\nraw: {results.Select((float res) => res.ToString()).ToLineList()}");
				});
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	[DebugAction("Autotests", "Battle Royale All PawnKinds", false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void BattleRoyaleAllPawnKinds()
	{
		ArenaUtility.PerformBattleRoyale(DefDatabase<PawnKindDef>.AllDefs);
	}

	[DebugAction("Autotests", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void BattleRoyaleHumanlikes()
	{
		ArenaUtility.PerformBattleRoyale(DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => k.RaceProps.Humanlike));
	}

	[DebugAction("Autotests", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
	private static void BattleRoyaleByDamagetype()
	{
		PawnKindDef[] array = new PawnKindDef[2]
		{
			PawnKindDefOf.Colonist,
			PawnKindDefOf.Muffalo
		};
		IEnumerable<ToolCapacityDef> enumerable = DefDatabase<ToolCapacityDef>.AllDefsListForReading.Where((ToolCapacityDef tc) => tc != ToolCapacityDefOf.KickMaterialInEyes);
		Func<PawnKindDef, ToolCapacityDef, string> func = (PawnKindDef pkd, ToolCapacityDef dd) => $"{pkd.label}_{dd.defName}";
		if (pawnKindsForDamageTypeBattleRoyale == null)
		{
			pawnKindsForDamageTypeBattleRoyale = new List<PawnKindDef>();
			PawnKindDef[] array2 = array;
			foreach (PawnKindDef pawnKindDef in array2)
			{
				foreach (ToolCapacityDef toolType in enumerable)
				{
					string text = func(pawnKindDef, toolType);
					ThingDef thingDef = Gen.MemberwiseClone(pawnKindDef.race);
					thingDef.defName = text;
					thingDef.label = text;
					thingDef.tools = new List<Tool>(pawnKindDef.race.tools.Select(delegate(Tool tool)
					{
						Tool tool2 = Gen.MemberwiseClone(tool);
						tool2.capacities = new List<ToolCapacityDef>();
						tool2.capacities.Add(toolType);
						return tool2;
					}));
					PawnKindDef pawnKindDef2 = Gen.MemberwiseClone(pawnKindDef);
					pawnKindDef2.defName = text;
					pawnKindDef2.label = text;
					pawnKindDef2.race = thingDef;
					pawnKindsForDamageTypeBattleRoyale.Add(pawnKindDef2);
				}
			}
		}
		ArenaUtility.PerformBattleRoyale(pawnKindsForDamageTypeBattleRoyale);
	}
}
