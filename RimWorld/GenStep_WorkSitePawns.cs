using System.Linq;
using RimWorld.BaseGen;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class GenStep_WorkSitePawns : GenStep
{
	public override int SeedPart => 237483478;

	public static int GetEnemiesCount(Site site, SitePartParams parms, PawnGroupKindDef workerGroupKind)
	{
		int pawnGroupMakerSeed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms);
		return PawnGroupMakerUtility.GeneratePawnKindsExample(GroupMakerParmsWorkers(site.Tile, site.Faction, parms.threatPoints, pawnGroupMakerSeed, workerGroupKind)).Count() + PawnGroupMakerUtility.GeneratePawnKindsExample(GroupMakerParmsFighters(site.Tile, site.Faction, parms.threatPoints, pawnGroupMakerSeed)).Count();
	}

	private static PawnGroupMakerParms GroupMakerParmsWorkers(PlanetTile tile, Faction faction, float points, int seed, PawnGroupKindDef workerGroupKind)
	{
		float a = points / 2f;
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
		{
			groupKind = workerGroupKind,
			tile = tile,
			faction = faction,
			inhabitants = true,
			seed = seed
		};
		pawnGroupMakerParms.points = Mathf.Max(a, faction.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind, pawnGroupMakerParms));
		return pawnGroupMakerParms;
	}

	private static PawnGroupMakerParms GroupMakerParmsFighters(PlanetTile tile, Faction faction, float points, int seed)
	{
		float a = points / 2f;
		PawnGroupKindDef groupKindDef = PawnGroupKindDefOf.Combat;
		if (!faction.def.pawnGroupMakers.Any((PawnGroupMaker maker) => maker.kindDef == groupKindDef))
		{
			groupKindDef = PawnGroupKindDefOf.Settlement;
		}
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
		{
			groupKind = groupKindDef,
			tile = tile,
			faction = faction,
			inhabitants = true,
			generateFightersOnly = true,
			seed = seed
		};
		pawnGroupMakerParms.points = Mathf.Max(a, faction.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind, pawnGroupMakerParms));
		return pawnGroupMakerParms;
	}

	public override void Generate(Map map, GenStepParams parms)
	{
		IntVec3 baseCenter;
		if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out var var))
		{
			baseCenter = var.CenterCell;
			Log.Error("No rect of interest set when running GenStep_WorkSitePawns!");
		}
		else
		{
			baseCenter = map.Center;
		}
		Faction faction = parms.sitePart.site.Faction;
		Lord singlePawnLord = LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, baseCenter, 25000), map);
		TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
		ResolveParams resolveParams = default(ResolveParams);
		resolveParams.rect = var;
		resolveParams.faction = faction;
		resolveParams.singlePawnLord = singlePawnLord;
		resolveParams.singlePawnSpawnCellExtraPredicate = (IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms);
		int pawnGroupMakerSeed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
		resolveParams.pawnGroupMakerParams = GroupMakerParmsWorkers(map.Tile, faction, parms.sitePart.parms.threatPoints, pawnGroupMakerSeed, ((SitePartWorker_WorkSite)parms.sitePart.def.Worker).WorkerGroupKind);
		resolveParams.pawnGroupKindDef = resolveParams.pawnGroupMakerParams.groupKind;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("pawnGroup", resolveParams);
		ResolveParams resolveParams2 = resolveParams;
		resolveParams2.pawnGroupMakerParams = GroupMakerParmsFighters(map.Tile, faction, parms.sitePart.parms.threatPoints, pawnGroupMakerSeed);
		resolveParams2.pawnGroupKindDef = resolveParams2.pawnGroupMakerParams.groupKind;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("pawnGroup", resolveParams2);
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.Generate();
	}
}
