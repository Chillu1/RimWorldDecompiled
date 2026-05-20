using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

public class JobDriver_BreastfeedCarryToMom : JobDriver
{
	private const TargetIndex BabyInd = TargetIndex.A;

	private const TargetIndex MomInd = TargetIndex.B;

	private Pawn Baby => (Pawn)base.TargetThingA;

	private Pawn Mom => (Pawn)base.TargetThingB;

	protected virtual bool MomMustBeImmobile => false;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(Baby, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(Mom, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
		this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
		AddFailCondition(() => (MomMustBeImmobile && !Mom.Downed && !Mom.IsPrisoner) || !ChildcareUtility.CanMomBreastfeedBaby(Mom, Baby, out var _));
		SetFinalizerJob((JobCondition condition) => (!pawn.IsCarryingPawn(Baby)) ? null : ChildcareUtility.MakeBringBabyToSafetyJob(pawn, Baby));
		Toil carryingBabyStart = Toils_General.Label();
		yield return Toils_Jump.JumpIf(carryingBabyStart, () => pawn.IsCarryingPawn(Baby));
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A);
		yield return carryingBabyStart;
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Reserve.Release(TargetIndex.A);
		yield return Toils_Reserve.Release(TargetIndex.B);
		yield return StartBreastfeedJobOnMom();
	}

	private Toil StartBreastfeedJobOnMom()
	{
		Toil toil = ToilMaker.MakeToil("StartBreastfeedJobOnMom");
		toil.initAction = delegate
		{
			Mom.jobs.SuspendCurrentJob(JobCondition.InterruptForced, cancelBusyStances: true, false);
			if (!pawn.carryTracker.innerContainer.TryTransferToContainer(pawn.carryTracker.CarriedThing, Mom.carryTracker.innerContainer))
			{
				Mom.jobs.EndCurrentJob(JobCondition.Incompletable);
				EndJobWith(JobCondition.Incompletable);
			}
			else
			{
				Pawn_JobTracker jobs = Mom.jobs;
				Job newJob = ChildcareUtility.MakeBreastfeedJob(Baby, Mom.CurrentBed());
				bool? keepCarryingThingOverride = true;
				jobs.StartJob(newJob, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, keepCarryingThingOverride);
			}
		};
		return toil;
	}
}
