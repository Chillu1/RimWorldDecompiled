using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;

namespace Verse;

public static class ArenaUtility
{
	public struct ArenaResult
	{
		public enum Winner
		{
			Other,
			Lhs,
			Rhs
		}

		public Winner winner;

		public int tickDuration;
	}

	private class ArenaSetState
	{
		public int live;
	}

	private const int liveSimultaneous = 15;

	public static bool ValidateArenaCapability()
	{
		if (Find.World.info.planetCoverage < 0.299f)
		{
			Log.Error("Planet coverage must be 30%+ to ensure a representative mix of biomes.");
			return false;
		}
		return true;
	}

	public static void BeginArenaFight(List<PawnKindDef> lhs, List<PawnKindDef> rhs, Action<ArenaResult> callback)
	{
		MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Debug_Arena);
		mapParent.Tile = TileFinder.RandomSettlementTileFor(Faction.OfPlayer, mustBeAutoChoosable: true, (PlanetTile tile) => lhs.Concat(rhs).Any((PawnKindDef pawnkind) => Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, pawnkind.race)));
		mapParent.SetFaction(Faction.OfPlayer);
		Find.WorldObjects.Add(mapParent);
		Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, new IntVec3(50, 1, 50), null);
		MultipleCaravansCellFinder.FindStartingCellsFor2Groups(orGenerateMap, out var first, out var second);
		List<Pawn> lhs2 = SpawnPawnSet(orGenerateMap, lhs, first, Faction.OfAncients);
		List<Pawn> rhs2 = SpawnPawnSet(orGenerateMap, rhs, second, Faction.OfAncientsHostile);
		DebugArena component = mapParent.GetComponent<DebugArena>();
		component.lhs = lhs2;
		component.rhs = rhs2;
		component.callback = callback;
	}

	public static List<Pawn> SpawnPawnSet(Map map, List<PawnKindDef> kinds, IntVec3 spot, Faction faction)
	{
		List<Pawn> list = new List<Pawn>();
		for (int i = 0; i < kinds.Count; i++)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(kinds[i], faction);
			IntVec3 loc = CellFinder.RandomClosewalkCellNear(spot, map, 12);
			GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
			list.Add(pawn);
		}
		LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(map.Center), map, list);
		return list;
	}

	private static bool ArenaFightQueue(List<PawnKindDef> lhs, List<PawnKindDef> rhs, Action<ArenaResult> callback, ArenaSetState state)
	{
		if (!ValidateArenaCapability())
		{
			return false;
		}
		if (state.live < 15)
		{
			BeginArenaFight(lhs, rhs, delegate(ArenaResult result)
			{
				state.live--;
				callback(result);
			});
			state.live++;
			return true;
		}
		return false;
	}

	public static void BeginArenaFightSet(int count, List<PawnKindDef> lhs, List<PawnKindDef> rhs, Action<ArenaResult> callback, Action report)
	{
		if (!ValidateArenaCapability())
		{
			return;
		}
		int remaining = count;
		ArenaSetState state = new ArenaSetState();
		for (int i = 0; i < count; i++)
		{
			Current.Game.GetComponent<GameComponent_DebugTools>().AddPerFrameCallback(() => ArenaFightQueue(lhs, rhs, delegate(ArenaResult result)
			{
				callback(result);
				int num = remaining - 1;
				remaining = num;
				if (remaining % 10 == 0)
				{
					report();
				}
			}, state));
		}
	}

	public static void PerformBattleRoyale(IEnumerable<PawnKindDef> kindsEnumerable)
	{
		if (!ValidateArenaCapability())
		{
			return;
		}
		List<PawnKindDef> kinds = kindsEnumerable.ToList();
		Dictionary<PawnKindDef, float> ratings = new Dictionary<PawnKindDef, float>();
		foreach (PawnKindDef item in kinds)
		{
			ratings[item] = EloUtility.CalculateRating(item.combatPower, 1500f, 60f);
		}
		int currentFights = 0;
		int completeFights = 0;
		Current.Game.GetComponent<GameComponent_DebugTools>().AddPerFrameCallback(delegate
		{
			if (currentFights >= 15)
			{
				return false;
			}
			PawnKindDef lhsDef = kinds.RandomElement();
			PawnKindDef rhsDef = kinds.RandomElement();
			float num = EloUtility.CalculateExpectation(ratings[lhsDef], ratings[rhsDef]);
			float num2 = 1f - num;
			float num3 = num;
			float num4 = Mathf.Min(num2, num3);
			num2 /= num4;
			num3 /= num4;
			float num5 = Mathf.Max(num2, num3);
			if (num5 > 40f)
			{
				return false;
			}
			float num6 = 40f / num5;
			float num7 = (float)Math.Exp(Rand.Range(0f, (float)Math.Log(num6)));
			num2 *= num7;
			num3 *= num7;
			List<PawnKindDef> lhs = Enumerable.Repeat(lhsDef, GenMath.RoundRandom(num2)).ToList();
			List<PawnKindDef> rhs = Enumerable.Repeat(rhsDef, GenMath.RoundRandom(num3)).ToList();
			int num8 = currentFights + 1;
			currentFights = num8;
			BeginArenaFight(lhs, rhs, delegate(ArenaResult result)
			{
				int num9 = currentFights - 1;
				currentFights = num9;
				num9 = completeFights + 1;
				completeFights = num9;
				if (result.winner != ArenaResult.Winner.Other)
				{
					float teamA = ratings[lhsDef];
					float teamB = ratings[rhsDef];
					float kfactor = 8f * Mathf.Pow(0.5f, Time.realtimeSinceStartup / 900f);
					EloUtility.Update(ref teamA, ref teamB, 0.5f, (result.winner == ArenaResult.Winner.Lhs) ? 1 : 0, kfactor);
					ratings[lhsDef] = teamA;
					ratings[rhsDef] = teamB;
					Log.Message(string.Format("Scores after {0} trials:\n\n{1}", completeFights, (from v in ratings
						orderby v.Value
						select string.Format("  {0}: {1}->{2} (rating {2})", v.Key.label, v.Key.combatPower, EloUtility.CalculateLinearScore(v.Value, 1500f, 60f).ToString("F0"), v.Value.ToString("F0"))).ToLineList()));
				}
			});
			return false;
		});
	}
}
