using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_PsychicRitual : LordJob
{
	public PsychicRitualDef def;

	public PsychicRitualRoleAssignments assignments;

	public float points;

	public override AcceptanceReport AllowsDrafting(Pawn pawn)
	{
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return base.AllowsDrafting(pawn);
		}
		return lordToil_PsychicRitual.RitualData.psychicRitual.def.AllowsDrafting(pawn);
	}

	public LordJob_PsychicRitual(PsychicRitualDef def, PsychicRitualRoleAssignments assignments, float points = -1f)
	{
		this.def = def;
		this.assignments = assignments;
		this.points = points;
	}

	protected LordJob_PsychicRitual()
	{
	}

	public override StateGraph CreateGraph()
	{
		if (!ModLister.CheckAnomaly("Psychic ritual"))
		{
			return null;
		}
		StateGraph stateGraph = new StateGraph();
		stateGraph.AddToil(new LordToil_PsychicRitual(def, assignments));
		return stateGraph;
	}

	public override string GetReport(Pawn pawn)
	{
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return base.GetJobReport(pawn);
		}
		return lordToil_PsychicRitual.RitualData.psychicRitualToil.GetPawnReport(lordToil_PsychicRitual.RitualData.psychicRitual, null, pawn)?.CapitalizeFirst();
	}

	public override string GetJobReport(Pawn pawn)
	{
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return base.GetJobReport(pawn);
		}
		return lordToil_PsychicRitual.RitualData.psychicRitualToil.GetJobReport(lordToil_PsychicRitual.RitualData.psychicRitual, null, pawn)?.CapitalizeFirst();
	}

	public override AcceptanceReport AllowsFloatMenu(Pawn pawn)
	{
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return base.AllowsFloatMenu(pawn);
		}
		return lordToil_PsychicRitual.RitualData.psychicRitualToil.AllowsFloatMenu(lordToil_PsychicRitual.RitualData.psychicRitual, null, pawn);
	}

	public override bool BlocksSocialInteraction(Pawn pawn)
	{
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return base.BlocksSocialInteraction(pawn);
		}
		return lordToil_PsychicRitual.RitualData.psychicRitualToil.BlocksSocialInteraction(lordToil_PsychicRitual.RitualData.psychicRitual, null, pawn);
	}

	public override bool DutyActiveWhenDown(Pawn pawn)
	{
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return base.BlocksSocialInteraction(pawn);
		}
		return lordToil_PsychicRitual.RitualData.psychicRitualToil.DutyActiveWhenDown(lordToil_PsychicRitual.RitualData.psychicRitual, null, pawn);
	}

	public override AcceptanceReport AbilityAllowed(Ability ability)
	{
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return base.AbilityAllowed(ability);
		}
		return lordToil_PsychicRitual.RitualData.psychicRitualToil.AbilityAllowed(lordToil_PsychicRitual.RitualData.psychicRitual, null, ability);
	}

	public override bool ShouldRemovePawn(Pawn pawn, PawnLostCondition reason)
	{
		if (reason == PawnLostCondition.LordRejected)
		{
			return true;
		}
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return base.ShouldRemovePawn(pawn, reason);
		}
		return lordToil_PsychicRitual.RitualData.psychicRitualToil.ShouldRemovePawn(lordToil_PsychicRitual.RitualData.psychicRitual, null, pawn, reason);
	}

	public override ThinkResult Notify_DutyConstantResult(ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return base.Notify_DutyConstantResult(result, pawn, issueParams);
		}
		return lordToil_PsychicRitual.RitualData.psychicRitualToil.Notify_DutyConstantResult(lordToil_PsychicRitual.RitualData.psychicRitual, null, result, pawn, issueParams);
	}

	public override ThinkResult Notify_DutyResult(ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		if (!(lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual))
		{
			return base.Notify_DutyResult(result, pawn, issueParams);
		}
		return lordToil_PsychicRitual.RitualData.psychicRitualToil.Notify_DutyResult(lordToil_PsychicRitual.RitualData.psychicRitual, null, result, pawn, issueParams);
	}

	public override void Notify_MapRemoved()
	{
		if (lord.CurLordToil is LordToil_PsychicRitual lordToil_PsychicRitual)
		{
			lordToil_PsychicRitual.RitualData.psychicRitual.CancelPsychicRitual(null);
		}
	}

	public override void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Deep.Look(ref assignments, "assignments");
		Scribe_Values.Look(ref points, "points", 0f);
	}

	public override bool EndPawnJobOnCleanup(Pawn pawn)
	{
		return pawn.CurJob?.lord == lord;
	}

	public override bool PrisonerSecure(Pawn pawn)
	{
		return true;
	}
}
