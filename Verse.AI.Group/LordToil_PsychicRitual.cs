using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI.Group;

public class LordToil_PsychicRitual : LordToil
{
	protected PsychicRitualDef def;

	protected PsychicRitualRoleAssignments assignments;

	protected Func<PsychicRitualToil> psychicRitualToilGenerator;

	public int iteration;

	public bool removeLordOnCancel = true;

	public PsychicRitualLordToilData RitualData => (PsychicRitualLordToilData)data;

	public override bool ShouldFail
	{
		get
		{
			if (!RitualData.done)
			{
				return base.ShouldFail;
			}
			return true;
		}
	}

	public LordToil_PsychicRitual(PsychicRitualDef def, PsychicRitualRoleAssignments assignments)
	{
		this.def = def;
		this.assignments = assignments;
	}

	public LordToil_PsychicRitual(Func<PsychicRitualToil> psychicRitualToilGenerator)
	{
		this.psychicRitualToilGenerator = psychicRitualToilGenerator;
	}

	protected virtual PsychicRitualToil GeneratePsychicRitualToil()
	{
		PsychicRitualToil psychicRitualToil;
		if (def != null)
		{
			psychicRitualToil = def.CreateGraph();
			if (psychicRitualToil == null)
			{
				throw new InvalidOperationException($"PsychicRitualDef {def} returned a null graph during CreateGraph().");
			}
		}
		else
		{
			psychicRitualToil = psychicRitualToilGenerator?.Invoke();
			if (psychicRitualToil == null)
			{
				throw new InvalidOperationException("PsychicRitualToil generator " + psychicRitualToilGenerator.ToStringSafe() + " returned a null toil when invoked.");
			}
		}
		return psychicRitualToil;
	}

	public override void Init()
	{
		data = new PsychicRitualLordToilData();
		RitualData.psychicRitual = new PsychicRitual
		{
			lord = lord,
			def = def,
			assignments = assignments
		};
		RitualData.iteration = iteration;
		RitualData.psychicRitualToil = GeneratePsychicRitualToil();
		RitualData.playerRitual = RitualData.psychicRitual.lord.faction == Faction.OfPlayer;
		RitualData.removeLordOnCancel = removeLordOnCancel;
		def.CalculateMaxPower(assignments, null, out var power);
		RitualData.psychicRitual.maxPower = power;
		RitualData.psychicRitual.Start();
		RitualData.psychicRitualToil.Start(RitualData.psychicRitual, null);
		if (RitualData.playerRitual)
		{
			Messages.Message(def.PsychicRitualBegunMessage(assignments).CapitalizeFirst(), assignments.Target, MessageTypeDefOf.NeutralEvent);
		}
	}

	public override void UpdateAllDuties()
	{
		RitualData.psychicRitualToil.UpdateAllDuties(RitualData.psychicRitual, null);
	}

	public override void LordToilTick()
	{
		if (RitualData.done)
		{
			return;
		}
		if (!RitualData.psychicRitual.succeeded)
		{
			RitualData.psychicRitual.def.RemoveIncapablePawns(RitualData.psychicRitual);
		}
		RitualData.psychicRitual.def.CheckPsychicRitualCancelConditions(RitualData.psychicRitual);
		if (!RitualData.done)
		{
			RitualData.done = RitualData.psychicRitualToil.Tick(RitualData.psychicRitual, null);
			if (RitualData.psychicRitual.canceled)
			{
				lord.ReceiveMemo("PsychicRitualCanceled");
			}
			else if (RitualData.done)
			{
				RitualCompleted();
			}
		}
	}

	private void RitualCompleted()
	{
		if (RitualData.playerRitual)
		{
			Find.IdeoManager.lastPsychicRitualPerformedTick = Find.TickManager.TicksGame;
			Messages.Message(RitualData.psychicRitual.def.PsychicRitualCompletedMessage().CapitalizeFirst(), assignments.Target, MessageTypeDefOf.NeutralEvent);
		}
		Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.PsychicRitualPerformed));
		foreach (Pawn allAssignedPawn in assignments.AllAssignedPawns)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.InvolvedInPsychicRitual, allAssignedPawn.Named(HistoryEventArgsNames.Doer)));
		}
		lord.ReceiveMemo("PsychicRitualCompleted" + RitualData.iteration);
	}

	public override void Cleanup()
	{
		RitualData.psychicRitualToil.End(RitualData.psychicRitual, null, success: false);
		RitualData.done = true;
	}

	public override IEnumerable<Gizmo> GetPawnGizmos(Pawn pawn)
	{
		return RitualData.psychicRitualToil.GetPawnGizmos(RitualData.psychicRitual, null, pawn);
	}

	public override IEnumerable<Gizmo> GetBuildingGizmos(Building building)
	{
		return RitualData.psychicRitualToil.GetBuildingGizmos(RitualData.psychicRitual, null, building);
	}

	public override void Notify_PawnLost(Pawn victim, PawnLostCondition condition)
	{
		PsychicRitual psychicRitual = RitualData.psychicRitual;
		psychicRitual.Notify_PawnLost(victim, condition);
		RitualData.psychicRitualToil.Notify_PawnLost(psychicRitual, null, victim, condition);
		PsychicRitualRoleDef psychicRitualRoleDef = assignments.RoleForPawn(victim);
		if (psychicRitualRoleDef == null || psychicRitualRoleDef.removeOnLost)
		{
			psychicRitual.assignments.RemoveParticipant(victim);
		}
	}

	public override void Notify_PawnJobDone(Pawn pawn, JobCondition condition)
	{
		if (pawn.CurJob.lord == lord)
		{
			RitualData.psychicRitualToil.Notify_PawnJobDone(RitualData.psychicRitual, null, pawn, pawn.CurJob, condition);
			(RitualData.psychicRitual.assignments?.RoleForPawn(pawn))?.Notify_PawnJobDone(RitualData.psychicRitual, pawn, condition);
		}
	}

	public override void Notify_BuildingLost(Building building)
	{
		RitualData.psychicRitualToil.Notify_BuildingLost(RitualData.psychicRitual, null, building);
	}

	public override void Notify_CorpseLost(Corpse corpse)
	{
		RitualData.psychicRitualToil.Notify_CorpseLost(RitualData.psychicRitual, null, corpse);
	}

	public override void Notify_ReachedDutyLocation(Pawn pawn)
	{
		RitualData.psychicRitualToil.Notify_ReachedDutyLocation(RitualData.psychicRitual, null, pawn);
	}

	public override void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
	{
		RitualData.psychicRitualToil.Notify_ConstructionFailed(RitualData.psychicRitual, null, pawn, frame, newBlueprint);
	}

	public override void Notify_BuildingSpawnedOnMap(Building building)
	{
		RitualData.psychicRitualToil.Notify_BuildingSpawnedOnMap(RitualData.psychicRitual, null, building);
	}

	public override void Notify_BuildingDespawnedOnMap(Building building)
	{
		RitualData.psychicRitualToil.Notify_BuildingDespawnedOnMap(RitualData.psychicRitual, null, building);
	}
}
