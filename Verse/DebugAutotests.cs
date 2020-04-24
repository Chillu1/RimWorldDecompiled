using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public static class DebugAutotests
	{
		private static List<PawnKindDef> pawnKindsForDamageTypeBattleRoyale;

		[DebugAction("Autotests", "Make colony (full)", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MakeColonyFull()
		{
			Autotests_ColonyMaker.MakeColony_Full();
		}

		[DebugAction("Autotests", "Make colony (animals)", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MakeColonyAnimals()
		{
			Autotests_ColonyMaker.MakeColony_Animals();
		}

		[DebugAction("Autotests", "Test force downed x100", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TestForceDownedx100()
		{
			int num = 0;
			Pawn pawn;
			while (true)
			{
				if (num < 100)
				{
					PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
					pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
					GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.CurrentMap), Find.CurrentMap), Find.CurrentMap);
					HealthUtility.DamageUntilDowned(pawn);
					if (pawn.Dead)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			Log.Error("Pawn died while force downing: " + pawn + " at " + pawn.Position);
		}

		[DebugAction("Autotests", "Test force kill x100", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TestForceKillx100()
		{
			int num = 0;
			Pawn pawn;
			while (true)
			{
				if (num < 100)
				{
					PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
					pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
					GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.CurrentMap), Find.CurrentMap), Find.CurrentMap);
					HealthUtility.DamageUntilDead(pawn);
					if (!pawn.Dead)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			Log.Error("Pawn died not die: " + pawn + " at " + pawn.Position);
		}

		[DebugAction("Autotests", "Test Surgery fail catastrophic x100", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TestSurgeryFailCatastrophicx100()
		{
			for (int i = 0; i < 100; i++)
			{
				PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
				Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
				GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.CurrentMap), Find.CurrentMap), Find.CurrentMap);
				pawn.health.forceIncap = true;
				BodyPartRecord part = pawn.health.hediffSet.GetNotMissingParts().RandomElement();
				HealthUtility.GiveInjuriesOperationFailureCatastrophic(pawn, part);
				pawn.health.forceIncap = false;
				if (pawn.Dead)
				{
					Log.Error("Pawn died: " + pawn + " at " + pawn.Position);
				}
			}
		}

		[DebugAction("Autotests", "Test Surgery fail ridiculous x100", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TestSurgeryFailRidiculousx100()
		{
			for (int i = 0; i < 100; i++)
			{
				PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
				Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
				GenSpawn.Spawn(pawn, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.CurrentMap), Find.CurrentMap), Find.CurrentMap);
				pawn.health.forceIncap = true;
				HealthUtility.GiveInjuriesOperationFailureRidiculous(pawn);
				pawn.health.forceIncap = false;
				if (pawn.Dead)
				{
					Log.Error("Pawn died: " + pawn + " at " + pawn.Position);
				}
			}
		}

		[DebugAction("Autotests", "Test generate pawn x1000", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TestGeneratePawnx1000()
		{
			float[] array = new float[10]
			{
				10f,
				20f,
				50f,
				100f,
				200f,
				500f,
				1000f,
				2000f,
				5000f,
				1E+20f
			};
			int[] array2 = new int[array.Length];
			for (int i = 0; i < 1000; i++)
			{
				PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
				PerfLogger.Reset();
				Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
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
			for (int j = 0; j < array2.Length; j++)
			{
				stringBuilder.AppendLine($"<{array[j]}ms: {array2[j]}");
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugAction("Autotests", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void CheckRegionListers()
		{
			Autotests_RegionListers.CheckBugs(Find.CurrentMap);
		}

		[DebugAction("Autotests", "Test time-to-down", allowedGameStates = AllowedGameStates.PlayingOnMap)]
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
						if (result.winner != 0)
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

		[DebugAction("Autotests", "Battle Royale All PawnKinds", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void BattleRoyaleAllPawnKinds()
		{
			ArenaUtility.PerformBattleRoyale(DefDatabase<PawnKindDef>.AllDefs);
		}

		[DebugAction("Autotests", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void BattleRoyaleHumanlikes()
		{
			ArenaUtility.PerformBattleRoyale(DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => k.RaceProps.Humanlike));
		}

		[DebugAction("Autotests", null, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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
}
