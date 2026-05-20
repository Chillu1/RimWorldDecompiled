using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI.Group;

public class PsychicRitualGraph : PsychicRitualToil
{
	public const int EndPsychicRitualIndex = -1;

	protected const int MaxJumpsPerTick = 1000;

	protected const int MaxGraphDepth = 1000;

	protected int? nextToilIndexOverride;

	protected int currentToilIndex;

	protected List<PsychicRitualToil> toils;

	protected PsychicRitualGraph parent;

	public bool willAdvancePastLastToil = true;

	public PsychicRitualGraph Parent => parent;

	public PsychicRitualGraph RootGraph
	{
		get
		{
			PsychicRitualGraph psychicRitualGraph = this;
			for (int i = 0; i < 1000; i++)
			{
				if (psychicRitualGraph.parent != null)
				{
					psychicRitualGraph = psychicRitualGraph.parent;
					continue;
				}
				return psychicRitualGraph;
			}
			throw new InvalidOperationException("Max depth exceeded while trying to find root for PsychicRitualGraph " + ToString() + ". Is there a cycle?");
		}
	}

	public int ToilCount => toils.Count;

	public PsychicRitualToil CurrentToil => GetToil(currentToilIndex);

	public PsychicRitualGraph()
	{
	}

	public PsychicRitualGraph(IEnumerable<PsychicRitualToil> toils)
	{
		this.toils = new List<PsychicRitualToil>(toils);
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		this.parent = parent;
		toils = toils ?? psychicRitual.def.CreateToils(psychicRitual, this);
		PsychicRitualToilRunner.Start(GetToil(currentToilIndex), psychicRitual, this);
	}

	public override void End(PsychicRitual psychicRitual, PsychicRitualGraph parent, bool success)
	{
		PsychicRitualToilRunner.End(GetToil(currentToilIndex), psychicRitual, this, success);
	}

	public override bool Tick(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		int num = currentToilIndex;
		PsychicRitualToil toil = GetToil(currentToilIndex);
		for (int i = 0; i < 1000; i++)
		{
			if (toil == null)
			{
				return true;
			}
			bool flag = PsychicRitualToilRunner.Tick(toil, psychicRitual, this);
			if (!flag)
			{
				return false;
			}
			if (flag && !willAdvancePastLastToil && currentToilIndex + 1 == toils.Count)
			{
				return true;
			}
			if (DebugSettings.logPsychicRitualTransitions)
			{
				Log.Warning($"Toil {toil} has finished during tick {Find.TickManager.TicksGame}.");
			}
			PsychicRitualToilRunner.End(toil, psychicRitual, this, success: true);
			currentToilIndex = nextToilIndexOverride ?? (currentToilIndex + 1);
			nextToilIndexOverride = null;
			toil = GetToil(currentToilIndex);
			PsychicRitualToilRunner.Start(toil, psychicRitual, this);
			PsychicRitualToilRunner.UpdateAllDuties(toil, psychicRitual, this);
			if (toil == null)
			{
				continue;
			}
			foreach (Pawn allAssignedPawn in psychicRitual.assignments.AllAssignedPawns)
			{
				if (toil.ClearJobOnStart(psychicRitual, parent, allAssignedPawn))
				{
					allAssignedPawn?.jobs?.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
		}
		throw new InvalidOperationException($"Max iterations exceeded, starting from toil {num}: {GetToil(num).ToStringSafe()} " + $"and ending on toil {currentToilIndex}: {GetToil(num).ToStringSafe()}");
	}

	public override void UpdateAllDuties(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		PsychicRitualToilRunner.UpdateAllDuties(GetToil(currentToilIndex), psychicRitual, this);
	}

	public override void Notify_PawnLost(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn victim, PawnLostCondition condition)
	{
		GetToil(currentToilIndex)?.Notify_PawnLost(psychicRitual, this, victim, condition);
	}

	public override void Notify_PawnJobDone(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, Job job, JobCondition condition)
	{
		GetToil(currentToilIndex)?.Notify_PawnJobDone(psychicRitual, this, pawn, job, condition);
	}

	public override void Notify_BuildingLost(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
		GetToil(currentToilIndex)?.Notify_BuildingLost(psychicRitual, this, building);
	}

	public override void Notify_CorpseLost(PsychicRitual psychicRitual, PsychicRitualGraph parent, Corpse corpse)
	{
		GetToil(currentToilIndex)?.Notify_CorpseLost(psychicRitual, this, corpse);
	}

	public override void Notify_ReachedDutyLocation(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		GetToil(currentToilIndex)?.Notify_ReachedDutyLocation(psychicRitual, this, pawn);
	}

	public override void Notify_ConstructionFailed(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
	{
		GetToil(currentToilIndex)?.Notify_ConstructionFailed(psychicRitual, this, pawn, frame, newBlueprint);
	}

	public override void Notify_BuildingSpawnedOnMap(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
		GetToil(currentToilIndex)?.Notify_BuildingSpawnedOnMap(psychicRitual, this, building);
	}

	public override void Notify_BuildingDespawnedOnMap(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
		GetToil(currentToilIndex)?.Notify_BuildingDespawnedOnMap(psychicRitual, this, building);
	}

	public override ThinkResult Notify_DutyResult(PsychicRitual psychicRitual, PsychicRitualGraph parent, ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		return GetToil(currentToilIndex)?.Notify_DutyResult(psychicRitual, this, result, pawn, issueParams) ?? result;
	}

	public override ThinkResult Notify_DutyConstantResult(PsychicRitual psychicRitual, PsychicRitualGraph parent, ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		return GetToil(currentToilIndex)?.Notify_DutyConstantResult(psychicRitual, this, result, pawn, issueParams) ?? result;
	}

	public override string GetPawnReport(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return GetToil(currentToilIndex)?.GetPawnReport(psychicRitual, this, pawn);
	}

	public override string GetJobReport(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return GetToil(currentToilIndex)?.GetJobReport(psychicRitual, this, pawn);
	}

	public override string GetReport(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		return GetToil(currentToilIndex)?.GetReport(psychicRitual, this);
	}

	public override AcceptanceReport AbilityAllowed(PsychicRitual psychicRitual, PsychicRitualGraph parent, Ability ability)
	{
		return GetToil(currentToilIndex)?.AbilityAllowed(psychicRitual, this, ability) ?? base.AbilityAllowed(psychicRitual, parent, ability);
	}

	public override AcceptanceReport AllowsFloatMenu(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return GetToil(currentToilIndex)?.AllowsFloatMenu(psychicRitual, this, pawn) ?? base.AllowsFloatMenu(psychicRitual, parent, pawn);
	}

	public override bool BlocksSocialInteraction(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return GetToil(currentToilIndex)?.BlocksSocialInteraction(psychicRitual, this, pawn) ?? base.BlocksSocialInteraction(psychicRitual, parent, pawn);
	}

	public override bool DutyActiveWhenDown(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return GetToil(currentToilIndex)?.DutyActiveWhenDown(psychicRitual, this, pawn) ?? base.DutyActiveWhenDown(psychicRitual, parent, pawn);
	}

	public override IEnumerable<Gizmo> GetPawnGizmos(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		PsychicRitualToil toil = GetToil(currentToilIndex);
		if (toil == null)
		{
			return base.GetPawnGizmos(psychicRitual, parent, pawn);
		}
		return toil.GetPawnGizmos(psychicRitual, this, pawn);
	}

	public override IEnumerable<Gizmo> GetBuildingGizmos(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
		PsychicRitualToil toil = GetToil(currentToilIndex);
		if (toil == null)
		{
			return base.GetBuildingGizmos(psychicRitual, parent, building);
		}
		return toil.GetBuildingGizmos(psychicRitual, this, building);
	}

	public override void SetPawnDuty(Pawn pawn, PsychicRitual psychicRitual, PsychicRitualGraph parent, DutyDef def, LocalTargetInfo? focus = null, LocalTargetInfo? focusSecond = null, LocalTargetInfo? focusThird = null, string tag = null, float wanderRadius = 0f, Rot4? overrideFacing = null)
	{
		PsychicRitualToil toil = GetToil(currentToilIndex);
		if (toil == null)
		{
			base.SetPawnDuty(pawn, psychicRitual, parent, def, focus, focusSecond, focusThird, tag, wanderRadius);
		}
		else
		{
			toil.SetPawnDuty(pawn, psychicRitual, this, def, focus, focusSecond, focusThird, tag, wanderRadius);
		}
	}

	public override bool ShouldRemovePawn(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, PawnLostCondition condition)
	{
		return GetToil(currentToilIndex)?.ShouldRemovePawn(psychicRitual, this, pawn, condition) ?? base.ShouldRemovePawn(psychicRitual, parent, pawn, condition);
	}

	public override bool ClearJobOnStart(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return GetToil(currentToilIndex)?.ClearJobOnStart(psychicRitual, this, pawn) ?? base.ClearJobOnStart(psychicRitual, parent, pawn);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref nextToilIndexOverride, "nextToilIndexOverride");
		Scribe_Values.Look(ref currentToilIndex, "currentToilIndex", 0);
		Scribe_Collections.Look(ref toils, "toils", LookMode.Deep);
		Scribe_References.Look(ref parent, "parent");
	}

	public PsychicRitualToil GetToil(int toilIndex)
	{
		if (toils == null)
		{
			return null;
		}
		if (toilIndex == -1)
		{
			return null;
		}
		if (toilIndex == toils.Count)
		{
			return null;
		}
		if (currentToilIndex < 0 || currentToilIndex > toils.Count)
		{
			throw new InvalidOperationException($"Cannot start toil at index {currentToilIndex}; " + $"it is outside the range of valid toils [0, ${toils.Count})");
		}
		return toils[toilIndex];
	}

	public bool SetNextToil(PsychicRitualToil toil)
	{
		if (toils == null)
		{
			throw new InvalidOperationException("Cannot set next toil to " + toil.ToStringSafe() + "; there are no toils set.");
		}
		int num;
		if (toil == null)
		{
			num = -1;
		}
		else
		{
			num = toils.IndexOf(toil);
			if (num < 0)
			{
				throw new ArgumentException("Cannot set next toil to " + toil.ToStringSafe() + "; it is not in the list of toils.");
			}
		}
		nextToilIndexOverride = num;
		return true;
	}
}
