using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class RitualBehaviorWorker_GravshipLaunch : RitualBehaviorWorker
{
	public bool forceVisitorsToLeave = true;

	public bool boardColonyAnimals = true;

	public bool boardColonyMechs = true;

	private Dictionary<Pawn, bool> cachedCanReachGravship = new Dictionary<Pawn, bool>();

	private int cacheTick = -1;

	private List<Pawn> tmpPawns = new List<Pawn>();

	private List<Pawn> tmpPawnToEndJob = new List<Pawn>();

	public override bool ChecksReservations => false;

	public RitualBehaviorWorker_GravshipLaunch()
	{
	}

	public RitualBehaviorWorker_GravshipLaunch(RitualBehaviorDef def)
		: base(def)
	{
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref forceVisitorsToLeave, "forceVisitorsToLeave", defaultValue: true);
		Scribe_Values.Look(ref boardColonyAnimals, "boardColonyAnimals", defaultValue: true);
		Scribe_Values.Look(ref boardColonyMechs, "boardColonyMechs", defaultValue: true);
	}

	public override bool TargetStillAllowed(TargetInfo selectedTarget, LordJob_Ritual ritual)
	{
		if (!base.TargetStillAllowed(selectedTarget, ritual))
		{
			return false;
		}
		return true;
	}

	public override string ExpectedDuration(Precept_Ritual ritual, RitualRoleAssignments assignments, float quality)
	{
		return null;
	}

	private bool CanReachGravship(Pawn pawn, Building_GravEngine engine)
	{
		if (Find.TickManager.TicksGame != cacheTick)
		{
			cacheTick = Find.TickManager.TicksGame;
			cachedCanReachGravship.Clear();
		}
		if (cachedCanReachGravship.TryGetValue(pawn, out var value))
		{
			return value;
		}
		IntVec3 spot;
		bool flag = GravshipUtility.TryFindSpotOnGravship(pawn, engine, out spot);
		cachedCanReachGravship[pawn] = flag;
		return flag;
	}

	public override bool PawnCanFillRole(Pawn pawn, RitualRole role, out string reason, TargetInfo ritualTarget)
	{
		reason = null;
		Building_GravEngine building_GravEngine = ritualTarget.Thing.TryGetComp<CompPilotConsole>()?.engine;
		if (building_GravEngine == null)
		{
			Log.ErrorOnce("Could not find engine for gravship launch", 23184679);
			return false;
		}
		if (role == null)
		{
			if (!CanReachGravship(pawn, building_GravEngine))
			{
				reason = "NoPathToGravship".Translate();
				return false;
			}
		}
		else if (!pawn.IsPrisoner && !pawn.CanReach(ritualTarget.Thing, PathEndMode.InteractionCell, Danger.Deadly))
		{
			reason = "NoPathToPilotConsole".Translate();
			return false;
		}
		return true;
	}

	public override void TryExecuteOn(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments, bool playerForced = false)
	{
		Building_GravEngine building_GravEngine = target.Thing.TryGetComp<CompPilotConsole>()?.engine;
		building_GravEngine.pawnsToBoard = new HashSet<Pawn>();
		building_GravEngine.pawnsToLeave = new HashSet<Pawn>();
		tmpPawns.Clear();
		tmpPawns.AddRange(target.Map.mapPawns.AllPawnsSpawned);
		foreach (Pawn tmpPawn in tmpPawns)
		{
			if (!tmpPawn.Downed)
			{
				if (forceVisitorsToLeave && tmpPawn.Faction != null && tmpPawn.Faction != Faction.OfPlayer && !tmpPawn.Faction.HostileTo(Faction.OfPlayer))
				{
					building_GravEngine.pawnsToLeave.Add(tmpPawn);
					tmpPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				else if (boardColonyAnimals && tmpPawn.IsColonyAnimal)
				{
					building_GravEngine.pawnsToBoard.Add(tmpPawn);
					tmpPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				else if (ModsConfig.BiotechActive && boardColonyMechs && tmpPawn.IsColonyMech)
				{
					building_GravEngine.pawnsToBoard.Add(tmpPawn);
					tmpPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
		}
		base.TryExecuteOn(target, organizer, ritual, obligation, assignments, playerForced);
	}

	public override void Cleanup(LordJob_Ritual ritual)
	{
		Building_GravEngine building_GravEngine = ritual.selectedTarget.Thing.TryGetComp<CompPilotConsole>()?.engine;
		if (building_GravEngine != null)
		{
			if (building_GravEngine.pawnsToBoard != null)
			{
				tmpPawnToEndJob.AddRange(building_GravEngine.pawnsToBoard);
			}
			if (building_GravEngine.pawnsToLeave != null)
			{
				tmpPawnToEndJob.AddRange(building_GravEngine.pawnsToLeave);
			}
			building_GravEngine.pawnsToBoard = null;
			building_GravEngine.pawnsToLeave = null;
		}
		foreach (Pawn item in tmpPawnToEndJob)
		{
			item.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
		tmpPawnToEndJob.Clear();
		base.Cleanup(ritual);
	}
}
