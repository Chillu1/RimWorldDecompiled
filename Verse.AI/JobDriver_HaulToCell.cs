using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobDriver_HaulToCell : JobDriver
{
	private bool forbiddenInitially;

	private const TargetIndex HaulableInd = TargetIndex.A;

	private const TargetIndex StoreCellInd = TargetIndex.B;

	private const int MinimumHaulingJobTicks = 30;

	public Thing ToHaul => job.GetTarget(TargetIndex.A).Thing;

	protected virtual bool DropCarriedThingIfNotTarget => false;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref forbiddenInitially, "forbiddenInitially", defaultValue: false);
	}

	public override string GetReport()
	{
		IntVec3 cell = job.targetB.Cell;
		Thing thing = null;
		if (pawn.CurJob == job && pawn.carryTracker.CarriedThing != null)
		{
			thing = pawn.carryTracker.CarriedThing;
		}
		else if (base.TargetThingA != null && base.TargetThingA.Spawned)
		{
			thing = base.TargetThingA;
		}
		if (thing == null)
		{
			return "ReportHaulingUnknown".Translate();
		}
		string text = null;
		SlotGroup slotGroup = cell.GetSlotGroup(base.Map);
		if (slotGroup != null)
		{
			text = slotGroup.parent.SlotYielderLabel();
		}
		if (text != null)
		{
			return "ReportHaulingTo".Translate(thing.Label, text.Named("DESTINATION"), thing.Named("THING"));
		}
		return "ReportHauling".Translate(thing.Label, thing);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		forbiddenInitially = base.TargetThingA != null && base.TargetThingA.IsForbidden(pawn);
	}

	protected virtual Toil BeforeDrop()
	{
		return Toils_General.Label();
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOnBurningImmobile(TargetIndex.B);
		this.FailOnForbidden(TargetIndex.B);
		if (!forbiddenInitially)
		{
			this.FailOnForbidden(TargetIndex.A);
		}
		yield return Toils_General.DoAtomic(delegate
		{
			startTick = Find.TickManager.TicksGame;
		});
		Toil reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A);
		yield return reserveTargetA;
		Toil postCarry = Toils_General.Label();
		Toil checkJumpPostCarry = Toils_Jump.JumpIf(postCarry, delegate
		{
			Thing carriedThing = pawn.carryTracker.CarriedThing;
			if (carriedThing == null)
			{
				return false;
			}
			return pawn.carryTracker.AvailableStackSpace(ToHaul.def) <= 0 || carriedThing == ToHaul;
		});
		yield return checkJumpPostCarry;
		yield return Toils_General.DoAtomic(delegate
		{
			if (DropCarriedThingIfNotTarget && pawn.IsCarrying())
			{
				if (DebugViewSettings.logCarriedBetweenJobs)
				{
					Log.Message($"Dropping {pawn.carryTracker.CarriedThing} because it is not the designated Thing to haul.");
				}
				pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
			}
		});
		Toil toilGoto = null;
		toilGoto = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, canGotoSpawnedParent: true).FailOnSomeonePhysicallyInteracting(TargetIndex.A).FailOn((Func<bool>)delegate
		{
			Pawn actor = toilGoto.actor;
			Job curJob = actor.jobs.curJob;
			if (curJob.haulMode == HaulMode.ToCellStorage)
			{
				Thing thing = curJob.GetTarget(TargetIndex.A).Thing;
				if (!actor.jobs.curJob.GetTarget(TargetIndex.B).Cell.IsValidStorageFor(base.Map, thing))
				{
					return true;
				}
			}
			return false;
		});
		yield return toilGoto;
		yield return checkJumpPostCarry;
		yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true, failIfStackCountLessThanJobCount: false, reserve: true, HaulAIUtility.IsInHaulableInventory(ToHaul));
		yield return postCarry;
		if (job.haulOpportunisticDuplicates)
		{
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveTargetA, TargetIndex.A, TargetIndex.B);
		}
		Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
		yield return carryToCell;
		yield return PossiblyDelay();
		yield return BeforeDrop();
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: true);
	}

	private Toil PossiblyDelay()
	{
		Toil toil = ToilMaker.MakeToil("PossiblyDelay");
		toil.atomicWithPrevious = true;
		toil.tickIntervalAction = delegate
		{
			if (Find.TickManager.TicksGame >= startTick + 30)
			{
				ReadyForNextToil();
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		return toil;
	}
}
