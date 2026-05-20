using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_BringBabyToSafety : JobDriver
{
	private const TargetIndex BabyInd = TargetIndex.A;

	private const TargetIndex BabyBedInd = TargetIndex.B;

	private Pawn Baby => (Pawn)base.TargetThingA;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Baby, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		AddFailCondition(() => !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
		this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
		AddFailCondition(() => !ChildcareUtility.CanSuckle(Baby, out var _));
		if (!job.playerForced && Baby.Spawned && !Baby.ComfortableTemperatureAtCell(Baby.Position, Baby.Map) && (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(Baby)))
		{
			yield return Toils_General.Do(delegate
			{
				Messages.Message("MessageTakingBabyToSafeTemperature".Translate(pawn.Named("ADULT"), Baby.Named("BABY")), new LookTargets(pawn, Baby), MessageTypeDefOf.NeutralEvent);
			});
		}
		Toil findBedForBaby = FindBedForBaby();
		yield return Toils_Jump.JumpIf(findBedForBaby, () => pawn.IsCarryingPawn(Baby)).FailOn(() => !pawn.IsCarryingPawn(Baby) && (pawn.Downed || pawn.Drafted));
		foreach (Toil item in JobDriver_PickupToHold.Toils(this))
		{
			yield return item;
		}
		yield return findBedForBaby;
		yield return Toils_Reserve.ReserveDestinationOrThing(TargetIndex.B);
		yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell).FailOnInvalidOrDestroyed(TargetIndex.B).FailOnForbidden(TargetIndex.B)
			.FailOnSomeonePhysicallyInteracting(TargetIndex.B)
			.FailOnDestroyedNullOrForbidden(TargetIndex.A)
			.FailOn(() => !pawn.IsCarryingPawn(Baby) || pawn.Downed || pawn.Drafted);
		yield return Toils_Reserve.ReleaseDestinationOrThing(TargetIndex.B);
		yield return Toils_Bed.TuckIntoBed(TargetIndex.B, TargetIndex.A);
	}

	private Toil FindBedForBaby()
	{
		Toil toil = ToilMaker.MakeToil("FindBedForBaby");
		toil.initAction = delegate
		{
			LocalTargetInfo pack = ChildcareUtility.SafePlaceForBaby(Baby, pawn);
			LocalTargetInfo pack2 = LocalTargetInfo.Invalid;
			if (CaravanFormingUtility.IsFormingCaravanOrDownedPawnToBeTakenByCaravan(Baby))
			{
				pack2 = JobGiver_PrepareCaravan_GatherDownedPawns.FindRandomDropCell(pawn, Baby);
			}
			if (pack.IsValid)
			{
				if (pack2.IsValid && pack2.Cell.DistanceTo(pawn.Position) < pack.Cell.DistanceTo(pawn.Position))
				{
					toil.GetActor().CurJob.SetTarget(TargetIndex.B, pack2);
				}
				else
				{
					toil.GetActor().CurJob.SetTarget(TargetIndex.B, pack);
				}
			}
			else if (pack2.IsValid)
			{
				toil.GetActor().CurJob.SetTarget(TargetIndex.B, pack2);
			}
			else
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		};
		toil.FailOn(() => !pawn.IsCarryingPawn(Baby) || pawn.Downed || pawn.Drafted);
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}
}
