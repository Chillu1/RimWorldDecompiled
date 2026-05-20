using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_TendEntity : JobDriver
{
	private bool usesMedicine;

	private const TargetIndex PlatformIndex = TargetIndex.A;

	private const TargetIndex MedicineIndex = TargetIndex.B;

	private const int MaxMedicineReservations = 10;

	private Thing Platform => base.TargetThingA;

	private Pawn InnerPawn => (Platform as Building_HoldingPlatform)?.HeldPawn;

	private Thing MedicineUsed => job.targetB.Thing;

	protected bool IsMedicineInDoctorInventory
	{
		get
		{
			if (MedicineUsed != null)
			{
				return pawn.inventory.Contains(MedicineUsed);
			}
			return false;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref usesMedicine, "usesMedicine", defaultValue: false);
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		usesMedicine = MedicineUsed != null;
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(Platform, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (usesMedicine)
		{
			int num = pawn.Map.reservationManager.CanReserveStack(pawn, MedicineUsed, 10);
			if (num <= 0)
			{
				return false;
			}
			int stackCount = Mathf.Min(num, Medicine.GetMedicineCountToFullyHeal(InnerPawn));
			if (!pawn.Reserve(MedicineUsed, job, 10, stackCount, null, errorOnFailed))
			{
				return false;
			}
		}
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOn(delegate
		{
			Pawn innerPawn = InnerPawn;
			if (innerPawn == null || innerPawn.Destroyed)
			{
				return true;
			}
			if (MedicineUsed != null && innerPawn.playerSettings != null && !innerPawn.playerSettings.medCare.AllowsMedicine(MedicineUsed.def))
			{
				return true;
			}
			CompHoldingPlatformTarget compHoldingPlatformTarget = innerPawn.TryGetComp<CompHoldingPlatformTarget>();
			return (compHoldingPlatformTarget != null && compHoldingPlatformTarget.containmentMode == EntityContainmentMode.Release) ? true : false;
		});
		AddEndCondition(delegate
		{
			if (HealthAIUtility.ShouldBeTendedNowByPlayer(InnerPawn))
			{
				return JobCondition.Ongoing;
			}
			return (job.playerForced && InnerPawn.health.HasHediffsNeedingTend()) ? JobCondition.Ongoing : JobCondition.Succeeded;
		});
		Toil reserveMedicine = null;
		Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		if (usesMedicine)
		{
			List<Toil> list = JobDriver_TendPatient.CollectMedicineToils(pawn, InnerPawn, job, gotoToil, out reserveMedicine);
			foreach (Toil item in list)
			{
				yield return item;
			}
		}
		yield return gotoToil;
		int ticks = (int)(1f / pawn.GetStatValue(StatDefOf.MedicalTendSpeed) * 600f);
		Toil waitToil = Toils_General.WaitWith(TargetIndex.A, ticks, useProgressBar: true, maintainPosture: false, maintainSleep: false, TargetIndex.A);
		waitToil.activeSkill = () => SkillDefOf.Medicine;
		waitToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch).WithProgressBarToilDelay(TargetIndex.A).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
		yield return Toils_Jump.JumpIf(waitToil, () => !usesMedicine || !IsMedicineInDoctorInventory);
		yield return Toils_Tend.PickupMedicine(TargetIndex.B, InnerPawn).FailOnDestroyedOrNull(TargetIndex.B);
		yield return waitToil;
		yield return Toils_Tend.FinalizeTend(InnerPawn);
		if (usesMedicine)
		{
			yield return JobDriver_TendPatient.FindMoreMedicineToil(pawn, InnerPawn, TargetIndex.B, job, reserveMedicine);
		}
		yield return Toils_Jump.Jump(gotoToil);
	}

	public override string GetReport()
	{
		return JobUtility.GetResolvedJobReport(job.def.reportString, InnerPawn, job.targetB, job.targetC);
	}
}
