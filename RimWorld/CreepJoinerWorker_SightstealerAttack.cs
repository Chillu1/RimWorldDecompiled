using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class CreepJoinerWorker_SightstealerAttack : BaseCreepJoinerWorker
{
	private static readonly FloatRange SpawnPointScale = new FloatRange(0.25f, 0.65f);

	public override bool CanOccurOnDeath => true;

	public override void DoResponse(List<TargetInfo> looktargets, List<NamedArgument> namedArgs)
	{
		if (!base.Pawn.SpawnedOrAnyParentSpawned)
		{
			return;
		}
		if (!base.Pawn.Dead)
		{
			base.Pawn.SetFaction(Find.FactionManager.OfEntities);
			base.Pawn.guest.Recruitable = false;
			if (base.Pawn.GetLord() != null)
			{
				base.Pawn.GetLord().Notify_PawnLost(base.Pawn, PawnLostCondition.Undefined);
			}
			LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_AssaultColony(Faction.OfEntities, canKidnap: false, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false), base.Pawn.MapHeld).AddPawn(base.Pawn);
		}
		Lord lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_SightstealerAssault(), base.Pawn.MapHeld);
		float b = StorytellerUtility.DefaultThreatPointsNow(base.Pawn.MapHeld) * SpawnPointScale.RandomInRange;
		b = Mathf.Max(Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Sightstealers), b);
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			faction = Faction.OfEntities,
			groupKind = PawnGroupKindDefOf.Sightstealers,
			points = b,
			tile = base.Pawn.MapHeld.Tile
		}).ToList();
		foreach (Pair<List<Pawn>, IntVec3> item in PawnsArrivalModeWorkerUtility.SplitIntoRandomGroupsNearMapEdge(list, base.Pawn.MapHeld, arriveInPods: false))
		{
			foreach (Pawn item2 in item.First)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(item.Second, base.Pawn.MapHeld, 8);
				GenSpawn.Spawn(item2, loc, base.Pawn.MapHeld);
			}
		}
		foreach (Pawn item3 in list)
		{
			lord.AddPawn(item3);
		}
		SoundDefOf.Sightstealer_SummonedHowl.PlayOneShot(base.Pawn);
	}
}
