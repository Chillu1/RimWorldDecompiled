using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_ReimplantXenogerm : LordToil
{
	private const float WanderRadius = 10f;

	public override bool ShouldFail => Data.target == null;

	public LordToilData_ReimplantXenogerm Data => (LordToilData_ReimplantXenogerm)data;

	public LordToil_ReimplantXenogerm(IntVec3 spot)
	{
		data = new LordToilData_ReimplantXenogerm
		{
			waitSpot = spot
		};
	}

	public override void Init()
	{
		SetTarget();
	}

	private void SetTarget()
	{
		LordToilData_ReimplantXenogerm lordToilData_ReimplantXenogerm = Data;
		if (!lord.ownedPawns.TryRandomElementByWeight((Pawn x) => (!x.Destroyed && !x.Downed && !x.health.hediffSet.HasHediff(HediffDefOf.XenogermReplicating)) ? 1 : 0, out lordToilData_ReimplantXenogerm.target))
		{
			lord.ownedPawns.TryRandomElementByWeight((Pawn x) => (!x.Destroyed && !x.Downed) ? 1 : 0, out lordToilData_ReimplantXenogerm.target);
		}
		if (lordToilData_ReimplantXenogerm.target != null)
		{
			Hediff firstHediffOfDef = lordToilData_ReimplantXenogerm.target.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.XenogermReplicating);
			if (firstHediffOfDef != null)
			{
				lordToilData_ReimplantXenogerm.target.health.RemoveHediff(firstHediffOfDef);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelSanguophageWaitingToReimplant".Translate(), "LetterTextSanguophageWaitingToReimplant".Translate(lordToilData_ReimplantXenogerm.target.Named("PAWN"), 15000.ToStringTicksToPeriod().Named("DURATION")), LetterDefOf.PositiveEvent, lordToilData_ReimplantXenogerm.target);
		}
	}

	public override void UpdateAllDuties()
	{
		LordToilData_ReimplantXenogerm lordToilData_ReimplantXenogerm = Data;
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i] == lordToilData_ReimplantXenogerm.target)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Idle, lordToilData_ReimplantXenogerm.waitSpot);
			}
			else
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.WanderClose_NoNeeds, lordToilData_ReimplantXenogerm.waitSpot, 10f);
			}
		}
	}

	public override void DrawPawnGUIOverlay(Pawn pawn)
	{
		if (pawn == Data.target)
		{
			pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
		}
	}

	public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
	{
		if (p == Data.target)
		{
			SetTarget();
		}
	}

	public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn requester, Pawn current)
	{
		LordToilData_ReimplantXenogerm lordToilData_ReimplantXenogerm = Data;
		if (lordToilData_ReimplantXenogerm.target != requester || requester.genes == null || current.genes == null)
		{
			yield break;
		}
		string text = "AcceptXenogerm".Translate(requester);
		if (!current.CanReach(lordToilData_ReimplantXenogerm.target, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			yield return new FloatMenuOption(text + ": " + "CannotReach".Translate().CapitalizeFirst(), null);
			yield break;
		}
		if (current.IsQuestLodger())
		{
			yield return new FloatMenuOption(text + ": " + "MessageCannotImplantInTempFactionMembers".Translate(), null);
			yield break;
		}
		if (GeneUtility.SameXenotype(current, lordToilData_ReimplantXenogerm.target))
		{
			yield return new FloatMenuOption(text + ": " + "MessageCannotUseOnSameXenotype".Translate(current), null);
			yield break;
		}
		yield return new FloatMenuOption(text, delegate
		{
			current.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.GetReimplanted, requester), JobTag.Misc);
		});
	}
}
