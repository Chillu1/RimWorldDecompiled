using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI.Group;

public class PsychicRitualToil_Multiplex : PsychicRitualToil
{
	private Dictionary<PsychicRitualRoleDef, PsychicRitualToil> roleToils;

	private List<PsychicRitualToil> childToils;

	private PsychicRitualToil fallback;

	private List<PsychicRitualRoleDef> tmpRolesLoading = new List<PsychicRitualRoleDef>();

	private List<PsychicRitualToil> tmpToilsLoading = new List<PsychicRitualToil>();

	protected PsychicRitualToil_Multiplex()
	{
	}

	public PsychicRitualToil_Multiplex(Dictionary<PsychicRitualRoleDef, PsychicRitualToil> roleToils, PsychicRitualToil fallback = null)
	{
		this.roleToils = new Dictionary<PsychicRitualRoleDef, PsychicRitualToil>(roleToils);
		this.fallback = fallback;
		childToils = new List<PsychicRitualToil>(roleToils.Values);
		childToils.Add(fallback);
		childToils.RemoveDuplicates();
	}

	public PsychicRitualToil ToilForPawn(PsychicRitual psychicRitual, Pawn pawn)
	{
		return roleToils.GetWithFallback(psychicRitual.assignments.RoleForPawn(pawn), fallback);
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		foreach (PsychicRitualToil childToil in childToils)
		{
			childToil.Start(psychicRitual, parent);
		}
	}

	public override void End(PsychicRitual psychicRitual, PsychicRitualGraph parent, bool success)
	{
		foreach (PsychicRitualToil childToil in childToils)
		{
			childToil.End(psychicRitual, parent, success);
		}
	}

	public override bool Tick(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		bool flag = true;
		foreach (PsychicRitualToil childToil in childToils)
		{
			flag = childToil.Tick(psychicRitual, parent) && flag;
		}
		return flag;
	}

	public override AcceptanceReport AllowsFloatMenu(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return ToilForPawn(psychicRitual, pawn)?.AllowsFloatMenu(psychicRitual, parent, pawn) ?? base.AllowsFloatMenu(psychicRitual, parent, pawn);
	}

	public override bool BlocksSocialInteraction(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return ToilForPawn(psychicRitual, pawn)?.BlocksSocialInteraction(psychicRitual, parent, pawn) ?? base.BlocksSocialInteraction(psychicRitual, parent, pawn);
	}

	public override void SetPawnDuty(Pawn pawn, PsychicRitual psychicRitual, PsychicRitualGraph parent, DutyDef def, LocalTargetInfo? focus = null, LocalTargetInfo? focusSecond = null, LocalTargetInfo? focusThird = null, string tag = null, float wanderRadius = 0f, Rot4? overrideFacing = null)
	{
		PsychicRitualToil psychicRitualToil = ToilForPawn(psychicRitual, pawn);
		if (psychicRitualToil != null)
		{
			psychicRitualToil.SetPawnDuty(pawn, psychicRitual, parent, def, focus, focusSecond, focusThird, tag, wanderRadius);
		}
		else
		{
			base.SetPawnDuty(pawn, psychicRitual, parent, def, focus, focusSecond, focusThird, tag, wanderRadius);
		}
	}

	public override void UpdateAllDuties(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		foreach (PsychicRitualToil childToil in childToils)
		{
			childToil.UpdateAllDuties(psychicRitual, parent);
		}
	}

	public override bool DutyActiveWhenDown(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return ToilForPawn(psychicRitual, pawn)?.DutyActiveWhenDown(psychicRitual, parent, pawn) ?? base.DutyActiveWhenDown(psychicRitual, parent, pawn);
	}

	public override bool ClearJobOnStart(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return ToilForPawn(psychicRitual, pawn)?.ClearJobOnStart(psychicRitual, parent, pawn) ?? base.ClearJobOnStart(psychicRitual, parent, pawn);
	}

	public override string GetJobReport(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		PsychicRitualToil psychicRitualToil = ToilForPawn(psychicRitual, pawn);
		if (psychicRitualToil != null)
		{
			return psychicRitualToil.GetJobReport(psychicRitual, parent, pawn);
		}
		return base.GetJobReport(psychicRitual, parent, pawn);
	}

	public override IEnumerable<Gizmo> GetPawnGizmos(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		PsychicRitualToil psychicRitualToil = ToilForPawn(psychicRitual, pawn);
		if (psychicRitualToil != null)
		{
			return psychicRitualToil.GetPawnGizmos(psychicRitual, parent, pawn);
		}
		return base.GetPawnGizmos(psychicRitual, parent, pawn);
	}

	public override IEnumerable<Gizmo> GetBuildingGizmos(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
		return childToils.SelectMany((PsychicRitualToil toil) => toil.GetBuildingGizmos(psychicRitual, parent, building));
	}

	public override string GetPawnReport(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		PsychicRitualToil psychicRitualToil = ToilForPawn(psychicRitual, pawn);
		if (psychicRitualToil != null)
		{
			return psychicRitualToil.GetPawnReport(psychicRitual, parent, pawn);
		}
		return base.GetPawnReport(psychicRitual, parent, pawn);
	}

	public override string GetReport(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		return null;
	}

	public override bool ShouldRemovePawn(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, PawnLostCondition condition)
	{
		return ToilForPawn(psychicRitual, pawn)?.ShouldRemovePawn(psychicRitual, parent, pawn, condition) ?? base.ShouldRemovePawn(psychicRitual, parent, pawn, condition);
	}

	public override AcceptanceReport AbilityAllowed(PsychicRitual psychicRitual, PsychicRitualGraph parent, Ability ability)
	{
		foreach (PsychicRitualToil childToil in childToils)
		{
			AcceptanceReport result = childToil.AbilityAllowed(psychicRitual, parent, ability);
			if (!result.Accepted)
			{
				return result;
			}
		}
		return base.AbilityAllowed(psychicRitual, parent, ability);
	}

	public override void Notify_BuildingDespawnedOnMap(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
		foreach (PsychicRitualToil childToil in childToils)
		{
			childToil.Notify_BuildingDespawnedOnMap(psychicRitual, parent, building);
		}
	}

	public override void Notify_BuildingLost(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
		foreach (PsychicRitualToil childToil in childToils)
		{
			childToil.Notify_BuildingLost(psychicRitual, parent, building);
		}
	}

	public override void Notify_BuildingSpawnedOnMap(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
		foreach (PsychicRitualToil childToil in childToils)
		{
			childToil.Notify_BuildingSpawnedOnMap(psychicRitual, parent, building);
		}
	}

	public override void Notify_CorpseLost(PsychicRitual psychicRitual, PsychicRitualGraph parent, Corpse corpse)
	{
		foreach (PsychicRitualToil childToil in childToils)
		{
			childToil.Notify_CorpseLost(psychicRitual, parent, corpse);
		}
	}

	public override ThinkResult Notify_DutyConstantResult(PsychicRitual psychicRitual, PsychicRitualGraph parent, ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		return ToilForPawn(psychicRitual, pawn)?.Notify_DutyConstantResult(psychicRitual, parent, result, pawn, issueParams) ?? base.Notify_DutyConstantResult(psychicRitual, parent, result, pawn, issueParams);
	}

	public override ThinkResult Notify_DutyResult(PsychicRitual psychicRitual, PsychicRitualGraph parent, ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		return ToilForPawn(psychicRitual, pawn)?.Notify_DutyResult(psychicRitual, parent, result, pawn, issueParams) ?? base.Notify_DutyResult(psychicRitual, parent, result, pawn, issueParams);
	}

	public override void Notify_PawnJobDone(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, Job job, JobCondition condition)
	{
		PsychicRitualToil psychicRitualToil = ToilForPawn(psychicRitual, pawn);
		if (psychicRitualToil != null)
		{
			psychicRitualToil.Notify_PawnJobDone(psychicRitual, parent, pawn, job, condition);
		}
		else
		{
			base.Notify_PawnJobDone(psychicRitual, parent, pawn, job, condition);
		}
	}

	public override void Notify_ReachedDutyLocation(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		PsychicRitualToil psychicRitualToil = ToilForPawn(psychicRitual, pawn);
		if (psychicRitualToil != null)
		{
			psychicRitualToil.Notify_ReachedDutyLocation(psychicRitual, parent, pawn);
		}
		else
		{
			base.Notify_ReachedDutyLocation(psychicRitual, parent, pawn);
		}
	}

	public override void Notify_PawnLost(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, PawnLostCondition cond)
	{
		PsychicRitualToil psychicRitualToil = ToilForPawn(psychicRitual, pawn);
		if (psychicRitualToil != null)
		{
			psychicRitualToil.Notify_PawnLost(psychicRitual, parent, pawn, cond);
		}
		else
		{
			base.Notify_PawnLost(psychicRitual, parent, pawn, cond);
		}
	}

	public override void Notify_ConstructionFailed(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
	{
		PsychicRitualToil psychicRitualToil = ToilForPawn(psychicRitual, pawn);
		if (psychicRitualToil != null)
		{
			psychicRitualToil.Notify_ConstructionFailed(psychicRitual, parent, pawn, frame, newBlueprint);
		}
		else
		{
			base.Notify_ConstructionFailed(psychicRitual, parent, pawn, frame, newBlueprint);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref roleToils, "roleToils", LookMode.Def, LookMode.Reference, ref tmpRolesLoading, ref tmpToilsLoading);
		Scribe_Collections.Look(ref childToils, "childToils", LookMode.Deep);
		Scribe_References.Look(ref fallback, "fallback");
	}
}
