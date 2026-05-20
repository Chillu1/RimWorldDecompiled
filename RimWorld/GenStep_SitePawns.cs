using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class GenStep_SitePawns : GenStep
{
	public FactionDef factionDef;

	public PawnGroupKindDef pawnGroupKindDef;

	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterDesc;

	private static readonly IntRange Delay = new IntRange(2500, 5000);

	public override int SeedPart => 897875812;

	private PawnGroupMakerParms GroupMakerParms(PlanetTile tile, Faction faction, float points, int seed)
	{
		PawnGroupKindDef settlement = pawnGroupKindDef;
		if (!faction.def.pawnGroupMakers.Any((PawnGroupMaker maker) => maker.kindDef == pawnGroupKindDef))
		{
			settlement = PawnGroupKindDefOf.Settlement;
		}
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
		{
			groupKind = settlement,
			tile = tile,
			faction = faction,
			inhabitants = true,
			generateFightersOnly = true,
			seed = seed
		};
		pawnGroupMakerParms.points = Mathf.Max(points, faction.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind, pawnGroupMakerParms));
		return pawnGroupMakerParms;
	}

	public override void Generate(Map map, GenStepParams parms)
	{
		CellRect var;
		IntVec3 baseCenter = (MapGenerator.TryGetVar<CellRect>("RectOfInterest", out var) ? var.CenterCell : map.Center);
		Faction faction = Find.FactionManager.FirstFactionOfDef(factionDef);
		Lord lord = LordMaker.MakeNewLord(faction, new LordJob_SitePawns(faction, baseCenter, Delay.RandomInRange), map);
		int pawnGroupMakerSeed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
		PawnGroupMakerParms parms2 = GroupMakerParms(map.Tile, faction, parms.sitePart.parms.threatPoints, pawnGroupMakerSeed);
		CellRect var2 = MapGenerator.GetVar<CellRect>("SpawnRect");
		LookTargets lookTargets = new LookTargets();
		foreach (Pawn item in PawnGroupMakerUtility.GeneratePawns(parms2))
		{
			if (var2.TryFindRandomCell(out var cell, Validator))
			{
				GenSpawn.Spawn(item, cell, map);
				lord.AddPawn(item);
				lookTargets.targets.Add(item);
			}
		}
		if (lookTargets.Any)
		{
			Find.LetterStack.ReceiveLetter(letterLabel, letterDesc, LetterDefOf.ThreatSmall, lookTargets);
		}
		bool Validator(IntVec3 c)
		{
			return c.Standable(map);
		}
	}
}
