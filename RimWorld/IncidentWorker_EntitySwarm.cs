using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public abstract class IncidentWorker_EntitySwarm : IncidentWorker
{
	protected virtual FloatRange SwarmSizeVariance { get; } = new FloatRange(0.7f, 1.3f);

	protected abstract PawnGroupKindDef GroupKindDef { get; }

	protected virtual float TransformPoints(float points)
	{
		return points;
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (parms.points < Faction.OfEntities.def.MinPointsToGeneratePawnGroup(GroupKindDef))
		{
			return false;
		}
		return base.CanFireNowSub(parms);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		Map map = (Map)parms.target;
		int num = 0;
		IntVec3 result = IntVec3.Invalid;
		IntVec3 travelDest = IntVec3.Invalid;
		bool flag = false;
		while (!flag && num < 5)
		{
			RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Hostile);
			flag = RCellFinder.TryFindTravelDestFrom(result, map, out travelDest);
			num++;
		}
		if (!flag)
		{
			return false;
		}
		parms.spawnCenter = result;
		List<Pawn> list = GenerateEntities(parms, TransformPoints(parms.points));
		if (list.NullOrEmpty())
		{
			return false;
		}
		if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
		{
			AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
		}
		IntVec3 result2 = parms.spawnCenter;
		if (!result2.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out result2, map, CellFinder.EdgeRoadChance_Hostile))
		{
			return false;
		}
		Rot4 rot = Rot4.FromAngleFlat((map.Center - result2).AngleFlat);
		foreach (Pawn item in list)
		{
			IntVec3 loc = CellFinder.RandomClosewalkCellNear(result2, map, 10);
			GenSpawn.Spawn(item, loc, map, rot);
			QuestUtility.AddQuestTag(item, parms.questTag);
		}
		SendLetter(parms, list);
		Find.TickManager.slower.SignalForceNormalSpeedShort();
		LordMaker.MakeNewLord(parms.faction, GenerateLordJob(result, travelDest), map, list);
		return true;
	}

	protected abstract LordJob GenerateLordJob(IntVec3 entry, IntVec3 dest);

	protected virtual void SendLetter(IncidentParms parms, List<Pawn> entities)
	{
		SendStandardLetter(parms, entities);
	}

	protected virtual List<Pawn> GenerateEntities(IncidentParms parms, float points)
	{
		Map map = (Map)parms.target;
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
		{
			groupKind = GroupKindDef,
			tile = map.Tile,
			faction = Faction.OfEntities,
			points = points * SwarmSizeVariance.RandomInRange
		};
		pawnGroupMakerParms.points = Mathf.Max(pawnGroupMakerParms.points, Faction.OfEntities.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind) * 1.05f);
		return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
	}
}
