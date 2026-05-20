using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI.Group;

public abstract class PsychicRitualToil : IExposable, ILoadReferenceable
{
	private string uniqueId;

	public PsychicRitualToil()
	{
		uniqueId = $"PsychicRitualToil_{Find.UniqueIDsManager.GetNextPsychicRitualToilID()}";
	}

	public string GetUniqueLoadID()
	{
		return uniqueId;
	}

	public virtual void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
	}

	public virtual bool Tick(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		return true;
	}

	public virtual void UpdateAllDuties(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
	}

	public virtual void End(PsychicRitual psychicRitual, PsychicRitualGraph parent, bool success)
	{
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref uniqueId, "uniqueId");
	}

	public virtual string GetJobReport(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return null;
	}

	public virtual string GetPawnReport(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return psychicRitual.def.GetPawnReport(psychicRitual, pawn);
	}

	public virtual string GetReport(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		return null;
	}

	public virtual AcceptanceReport AllowsFloatMenu(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return psychicRitual.def.AllowsFloatMenu(pawn);
	}

	public virtual bool BlocksSocialInteraction(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return psychicRitual.def.BlocksSocialInteraction(pawn);
	}

	public virtual bool ClearJobOnStart(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return true;
	}

	public virtual AcceptanceReport AbilityAllowed(PsychicRitual psychicRitual, PsychicRitualGraph parent, Ability ability)
	{
		return psychicRitual.def.AbilityAllowed(ability);
	}

	public virtual IEnumerable<Gizmo> GetPawnGizmos(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		PsychicRitualRoleDef psychicRitualRoleDef = psychicRitual.assignments.RoleForPawn(pawn);
		if (psychicRitualRoleDef == null)
		{
			return Enumerable.Empty<Gizmo>();
		}
		return psychicRitualRoleDef.GetPawnGizmos(psychicRitual, pawn);
	}

	public virtual IEnumerable<Gizmo> GetBuildingGizmos(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
		return Enumerable.Empty<Gizmo>();
	}

	public virtual void SetPawnDuty(Pawn pawn, PsychicRitual psychicRitual, PsychicRitualGraph parent, DutyDef def, LocalTargetInfo? focus = null, LocalTargetInfo? focusSecond = null, LocalTargetInfo? focusThird = null, string tag = null, float wanderRadius = 0f, Rot4? overrideFacing = null)
	{
		if (pawn?.mindState != null)
		{
			pawn.mindState.duty = new PawnDuty(def, focus ?? LocalTargetInfo.Invalid, focusSecond ?? LocalTargetInfo.Invalid, focusThird ?? LocalTargetInfo.Invalid)
			{
				tag = tag,
				socialModeMaxOverride = RandomSocialMode.Off,
				source = this,
				locomotion = LocomotionUrgency.Sprint,
				wanderRadius = wanderRadius,
				overrideFacing = (overrideFacing ?? Rot4.Invalid)
			};
		}
	}

	public virtual bool ShouldRemovePawn(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, PawnLostCondition condition)
	{
		PsychicRitualRoleDef psychicRitualRoleDef = psychicRitual.assignments.RoleForPawn(pawn);
		if (psychicRitualRoleDef == null)
		{
			return true;
		}
		if (!psychicRitualRoleDef.ConditionAllowed(condition))
		{
			return true;
		}
		return false;
	}

	public virtual void Notify_PawnLost(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, PawnLostCondition cond)
	{
	}

	public virtual void Notify_PawnJobDone(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, Job job, JobCondition condition)
	{
	}

	public virtual void Notify_BuildingLost(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
	}

	public virtual void Notify_CorpseLost(PsychicRitual psychicRitual, PsychicRitualGraph parent, Corpse corpse)
	{
	}

	public virtual void Notify_ReachedDutyLocation(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
	}

	public virtual void Notify_ConstructionFailed(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
	{
	}

	public virtual void Notify_BuildingSpawnedOnMap(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
	}

	public virtual void Notify_BuildingDespawnedOnMap(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
	}

	public virtual ThinkResult Notify_DutyConstantResult(PsychicRitual psychicRitual, PsychicRitualGraph parent, ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		return result;
	}

	public virtual ThinkResult Notify_DutyResult(PsychicRitual psychicRitual, PsychicRitualGraph parent, ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		return result;
	}

	public virtual bool DutyActiveWhenDown(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		return false;
	}
}
