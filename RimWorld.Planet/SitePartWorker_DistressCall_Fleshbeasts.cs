using System.Collections.Generic;
using System.Linq;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class SitePartWorker_DistressCall_Fleshbeasts : SitePartWorker_DistressCall
{
	private const float CorpsePointFactor = 0.33f;

	private const int SpawnRadius = 20;

	private static readonly SimpleCurve FleshbeastsPointsModifierCurve = new SimpleCurve
	{
		new CurvePoint(100f, 100f),
		new CurvePoint(500f, 250f),
		new CurvePoint(1000f, 400f),
		new CurvePoint(5000f, 800f)
	};

	private const float ChanceForFleshbeastCorpsesAroundBurrow = 0.4f;

	private static readonly IntRange BurrowCorpseCountRange = new IntRange(1, 2);

	public override void PostMapGenerate(Map map)
	{
		Site site = map.Parent as Site;
		Faction faction = site.Faction ?? Find.FactionManager.RandomEnemyFaction();
		if (faction.IsPlayer)
		{
			faction = Find.FactionManager.RandomEnemyFaction();
		}
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			faction = faction,
			groupKind = PawnGroupKindDefOf.Settlement,
			points = SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange * 0.33f,
			tile = map.Tile
		}).ToList();
		float num = Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Fleshbeasts) * 1.05f;
		float num2 = Mathf.Max(FleshbeastsPointsModifierCurve.Evaluate(site.desiredThreatPoints), num);
		List<Pawn> fleshbeasts = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Fleshbeasts,
			points = Rand.Range(num, num2 * 0.33f),
			faction = Faction.OfEntities,
			raidStrategy = RaidStrategyDefOf.ImmediateAttack
		}).ToList();
		List<Pawn> list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Fleshbeasts,
			points = num2,
			faction = Faction.OfEntities,
			raidStrategy = RaidStrategyDefOf.ImmediateAttack
		}).ToList();
		SplitFleshbeasts(ref fleshbeasts);
		DistressCallUtility.SpawnCorpses(map, fleshbeasts, list, map.Center, 20);
		DistressCallUtility.SpawnCorpses(map, list, fleshbeasts.Concat(list2), map.Center, 20);
		DistressCallUtility.SpawnPawns(map, list2, map.Center, 20);
		ScatterCorpsesAroundPitBurrows(map, list);
	}

	private void ScatterCorpsesAroundPitBurrows(Map map, IEnumerable<Pawn> killers)
	{
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.PitBurrow))
		{
			if (Rand.Chance(0.4f))
			{
				int randomInRange = BurrowCorpseCountRange.RandomInRange;
				List<Pawn> fleshbeasts = new List<Pawn>();
				for (int i = 0; i < randomInRange; i++)
				{
					fleshbeasts.Add(PawnGenerator.GeneratePawn(FleshbeastUtility.AllFleshbeasts.RandomElement(), Faction.OfEntities));
				}
				SplitFleshbeasts(ref fleshbeasts);
				DistressCallUtility.SpawnCorpses(map, fleshbeasts, killers, item.Position, 3);
			}
		}
	}

	private void SplitFleshbeasts(ref List<Pawn> fleshbeasts)
	{
		List<Pawn> list = new List<Pawn>();
		foreach (Pawn fleshbeast in fleshbeasts)
		{
			foreach (Pawn item in FleshbeastUtility.SplitFleshbeast(fleshbeast))
			{
				list.Add(item);
			}
		}
		fleshbeasts = list;
	}
}
