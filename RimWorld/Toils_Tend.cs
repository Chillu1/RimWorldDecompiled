using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class Toils_Tend
{
	public const int MaxMedicineReservations = 10;

	public static Toil ReserveMedicine(TargetIndex ind, Pawn injured)
	{
		Toil toil = ToilMaker.MakeToil("ReserveMedicine");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Job curJob = actor.jobs.curJob;
			Thing thing = curJob.GetTarget(ind).Thing;
			int num = actor.Map.reservationManager.CanReserveStack(actor, thing, 10);
			if (num <= 0 || !actor.Reserve(thing, curJob, 10, Mathf.Min(num, Medicine.GetMedicineCountToFullyHeal(injured))))
			{
				toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	public static Toil PickupMedicine(TargetIndex ind, Pawn injured)
	{
		Toil toil = ToilMaker.MakeToil("PickupMedicine");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Job curJob = actor.jobs.curJob;
			Thing thing = curJob.GetTarget(ind).Thing;
			int num = Medicine.GetMedicineCountToFullyHeal(injured);
			if (actor.carryTracker.CarriedThing != null)
			{
				num -= actor.carryTracker.CarriedThing.stackCount;
			}
			int num2 = Mathf.Min(actor.Map.reservationManager.CanReserveStack(actor, thing, 10), num);
			if (num2 > 0)
			{
				actor.carryTracker.TryStartCarry(thing, num2);
			}
			curJob.count = num - num2;
			if (thing.Spawned)
			{
				toil.actor.Map.reservationManager.Release(thing, actor, curJob);
			}
			curJob.SetTarget(ind, actor.carryTracker.CarriedThing);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

	public static Toil FinalizeTend(Pawn patient)
	{
		Toil toil = ToilMaker.MakeToil("FinalizeTend");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Medicine medicine = (Medicine)actor.CurJob.targetB.Thing;
			if (actor.skills != null)
			{
				float num = (patient.RaceProps.Animal ? 175f : 500f);
				float num2 = medicine?.def.MedicineTendXpGainFactor ?? 0.5f;
				actor.skills.Learn(SkillDefOf.Medicine, num * num2);
			}
			TendUtility.DoTend(actor, patient, medicine);
			if (medicine != null && medicine.Destroyed)
			{
				actor.CurJob.SetTarget(TargetIndex.B, LocalTargetInfo.Invalid);
			}
			if (toil.actor.CurJob.endAfterTendedOnce)
			{
				actor.jobs.EndCurrentJob(JobCondition.Succeeded);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}
}
