using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class GenStep_GravshipWreckage : GenStep
{
	private static readonly FloatRange DefaultPointsRange = new FloatRange(340f, 1000f);

	private static readonly FloatRange DeadPointsRange = new FloatRange(340f, 600f);

	private const float WanderRadius = 11.9f;

	private static readonly IntRange ChunkGroups = new IntRange(2, 3);

	private static readonly IntRange ChunksPerGroup = new IntRange(1, 2);

	private static readonly IntRange ChunksDistRange = new IntRange(2, 4);

	private static readonly IntRange Panels = new IntRange(100, 100);

	private static readonly IntRange NumCratersRange = new IntRange(3, 6);

	private static readonly IntRange NumFilthRange = new IntRange(10, 15);

	public override int SeedPart => 26145111;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModsConfig.OdysseyActive)
		{
			CellRect rect;
			bool num = MapGenUtility.TryGetRandomClearRect(20, 20, out rect, -1, -1, RectValidator);
			IntVec3 intVec = (num ? (rect.CenterCell + IntVec3.SouthEast) : map.Center);
			SpawnCraters(map, intVec);
			SpawnFilth(map, intVec);
			SpawnShipChunks(map, intVec);
			GenSpawn.Spawn(ThingDefOf.GravEngine, intVec, map).questTags = parms.sitePart.site.questTags;
			SpawnCorpses(map, parms, intVec);
			SpawnPanels(map, intVec);
			SpawnDefenders(map, parms, intVec);
			if (num)
			{
				MapGenerator.UsedRects.Add(rect.ExpandedBy(8));
			}
		}
		bool RectValidator(CellRect r)
		{
			foreach (IntVec3 cell in r.Cells)
			{
				if (cell.GetEdifice(map) != null || !cell.GetAffordances(map).Contains(TerrainAffordanceDefOf.Medium) || MapGenerator.UsedRects.Any((CellRect a) => a.Overlaps(r)))
				{
					return false;
				}
			}
			return true;
		}
	}

	private static void SpawnCraters(Map map, IntVec3 root)
	{
		int randomInRange = NumCratersRange.RandomInRange;
		List<ThingDef> source = new List<ThingDef>
		{
			ThingDefOf.CraterSmall,
			ThingDefOf.CraterMedium,
			ThingDefOf.CraterLarge
		};
		for (int i = 0; i < randomInRange; i++)
		{
			ThingDef def = source.RandomElement();
			if (RCellFinder.TryFindRandomCellNearWith(root, (IntVec3 c) => Validator(c, def), map, out var result, 7, 40))
			{
				GenSpawn.Spawn(def, result, map);
				continue;
			}
			break;
		}
		bool Validator(IntVec3 cell, ThingDef thingDef)
		{
			return MapGenUtility.IsGoodSpawnCell(thingDef, cell, map);
		}
	}

	private static void SpawnFilth(Map map, IntVec3 root)
	{
		int randomInRange = NumFilthRange.RandomInRange;
		List<ThingDef> source = new List<ThingDef>
		{
			ThingDefOf.Filth_BlastMark,
			ThingDefOf.Filth_OilSmear
		};
		for (int i = 0; i < randomInRange; i++)
		{
			ThingDef def = source.RandomElement();
			if (RCellFinder.TryFindRandomCellNearWith(root, (IntVec3 c) => Validator(c, def), map, out var result, 7, 40))
			{
				FilthMaker.TryMakeFilth(result, map, def);
				continue;
			}
			break;
		}
		bool Validator(IntVec3 cell, ThingDef filthDef)
		{
			return FilthMaker.CanMakeFilth(cell, map, filthDef);
		}
	}

	private static void SpawnPanels(Map map, IntVec3 root)
	{
		int num = Panels.RandomInRange;
		ThingDef def = ThingDefOf.GravlitePanel;
		IntVec3 result;
		while (num > 0 && RCellFinder.TryFindRandomCellNearWith(root, Validator, map, out result, 5, 40))
		{
			Thing thing = GenSpawn.Spawn(def, result, map);
			thing.stackCount = Mathf.Min(num, def.stackLimit);
			thing.TrySetForbidden(value: true);
			num -= thing.stackCount;
		}
		bool Validator(IntVec3 cell)
		{
			return MapGenUtility.IsGoodSpawnCell(def, cell, map);
		}
	}

	private static void SpawnShipChunks(Map map, IntVec3 root)
	{
		int randomInRange = ChunkGroups.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			if (RCellFinder.TryFindRandomCellNearWith(root, Validator, map, out var result, 5, 40))
			{
				GenSpawn.SpawnIrregularLump(ThingDefOf.ShipChunk_Mech, result, map, ChunksPerGroup, ChunksDistRange, WipeMode.Vanish, Validator);
			}
		}
		bool Validator(IntVec3 cell)
		{
			return MapGenUtility.IsGoodSpawnCell(ThingDefOf.ShipChunk_Mech, cell, map);
		}
	}

	private void SpawnCorpses(Map map, GenStepParams parms, IntVec3 root)
	{
		foreach (Pawn pawn in GeneratePawns(parms, map, DeadPointsRange.RandomInRange))
		{
			if (!RCellFinder.TryFindRandomCellNearWith(root, (IntVec3 c) => Validator(c, pawn.def), map, out var result, 5, 40))
			{
				pawn.Destroy();
				break;
			}
			HealthUtility.SimulateKilled(pawn, DamageDefOf.Blunt);
			GenSpawn.Spawn(pawn.Corpse, result, map);
			pawn.Corpse.TrySetForbidden(value: true);
		}
		bool Validator(IntVec3 cell, ThingDef def)
		{
			return MapGenUtility.IsGoodSpawnCell(def, cell, map);
		}
	}

	private void SpawnDefenders(Map map, GenStepParams parms, IntVec3 root)
	{
		List<Pawn> list = new List<Pawn>();
		float points = ((parms.sitePart != null) ? parms.sitePart.parms.points : DefaultPointsRange.RandomInRange);
		foreach (Pawn pawn in GeneratePawns(parms, map, points))
		{
			if (!RCellFinder.TryFindRandomCellNearWith(root, (IntVec3 c) => Validator(c, pawn.def), map, out var result, 5, 40))
			{
				pawn.Destroy();
				break;
			}
			GenSpawn.Spawn(pawn, result, map);
			list.Add(pawn);
		}
		if (list.Any())
		{
			LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_DefendPoint(root, 11.9f), map, list);
		}
		bool Validator(IntVec3 cell, ThingDef def)
		{
			return MapGenUtility.IsGoodSpawnCell(def, cell, map);
		}
	}

	private IEnumerable<Pawn> GeneratePawns(GenStepParams parms, Map map, float points)
	{
		float b = Faction.OfMechanoids.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat);
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Combat,
			tile = map.Tile,
			faction = Faction.OfMechanoids,
			points = Mathf.Max(points, b)
		};
		if (parms.sitePart != null)
		{
			pawnGroupMakerParms.seed = Gen.HashCombineInt(SleepingMechanoidsSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms), (int)points);
		}
		return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
	}
}
