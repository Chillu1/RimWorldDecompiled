using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ComplexThreatWorker_RaidTerminal : ComplexThreatWorker
{
	private const string SignalPrefix = "RaidSignal";

	protected override bool CanResolveInt(ComplexResolveParams parms)
	{
		IntVec3 spawnPosition;
		if (base.CanResolveInt(parms) && TryFindRandomEnemyFaction(parms, out var _))
		{
			return ComplexUtility.TryFindRandomSpawnCell(ThingDefOf.AncientEnemyTerminal, parms.room, parms.map, out spawnPosition);
		}
		return false;
	}

	protected override void ResolveInt(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> outSpawnedThings)
	{
		ComplexUtility.TryFindRandomSpawnCell(ThingDefOf.AncientEnemyTerminal, parms.room, parms.map, out var spawnPosition);
		Thing thing = GenSpawn.Spawn(ThingDefOf.AncientEnemyTerminal, spawnPosition, parms.map);
		TryFindRandomEnemyFaction(parms, out var faction);
		float num = Mathf.Max(parms.points, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat) * 1.05f);
		IncidentParms incidentParms = new IncidentParms
		{
			forced = true,
			target = parms.map,
			points = num,
			faction = faction,
			raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
			raidStrategy = RaidStrategyDefOf.ImmediateAttack
		};
		CompHackable compHackable = thing.TryGetComp<CompHackable>();
		string text = compHackable.hackingCompletedSignal;
		if (text.NullOrEmpty())
		{
			text = (compHackable.hackingCompletedSignal = "RaidSignal" + Find.UniqueIDsManager.GetNextSignalTagID());
		}
		SignalAction_Incident obj = (SignalAction_Incident)ThingMaker.MakeThing(ThingDefOf.SignalAction_Incident);
		obj.incident = IncidentDefOf.RaidEnemy;
		obj.incidentParms = incidentParms;
		obj.signalTag = text;
		GenSpawn.Spawn(obj, parms.room.rects[0].CenterCell, parms.map);
		threatPointsUsed += num;
	}

	private bool TryFindRandomEnemyFaction(ComplexResolveParams parms, out Faction faction)
	{
		if (parms.hostileFaction != null && parms.hostileFaction.def.humanlikeFaction)
		{
			faction = parms.hostileFaction;
			return true;
		}
		faction = Find.FactionManager.RandomRaidableEnemyFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);
		return faction != null;
	}
}
