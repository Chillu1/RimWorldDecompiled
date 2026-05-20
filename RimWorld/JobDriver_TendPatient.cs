using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_TendPatient : JobDriver
{
	private bool usesMedicine;

	private PathEndMode pathEndMode;

	public const int BaseTendDuration = 600;

	private const int TicksBetweenSelfTendMotes = 100;

	private const TargetIndex MedicineIndex = TargetIndex.B;

	private const TargetIndex MedicineHolderIndex = TargetIndex.C;

	private static List<Toil> tmpCollectToils = new List<Toil>();

	protected Thing MedicineUsed => job.targetB.Thing;

	protected Pawn Deliveree => job.targetA.Pawn;

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

	protected Pawn_InventoryTracker MedicineHolderInventory => MedicineUsed?.ParentHolder as Pawn_InventoryTracker;

	protected Pawn OtherPawnMedicineHolder => job.targetC.Pawn;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref usesMedicine, "usesMedicine", defaultValue: false);
		Scribe_Values.Look(ref pathEndMode, "pathEndMode", PathEndMode.None);
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		usesMedicine = MedicineUsed != null;
		if (Deliveree == pawn)
		{
			pathEndMode = PathEndMode.OnCell;
		}
		else if (Deliveree.InBed())
		{
			pathEndMode = PathEndMode.InteractionCell;
		}
		else if (Deliveree != pawn)
		{
			pathEndMode = PathEndMode.ClosestTouch;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(Deliveree, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (MedicineUsed != null)
		{
			int num = pawn.Map.reservationManager.CanReserveStack(pawn, MedicineUsed, 10);
			if (num <= 0 || !pawn.Reserve(MedicineUsed, job, 10, Mathf.Min(num, Medicine.GetMedicineCountToFullyHeal(Deliveree)), null, errorOnFailed))
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
			if (MedicineUsed != null && pawn.Faction == Faction.OfPlayer && Deliveree.playerSettings != null && !Deliveree.playerSettings.medCare.AllowsMedicine(MedicineUsed.def))
			{
				return true;
			}
			return (pawn == Deliveree && pawn.Faction == Faction.OfPlayer && pawn.playerSettings != null && !pawn.playerSettings.selfTend) ? true : false;
		});
		AddEndCondition(delegate
		{
			if (pawn.Faction == Faction.OfPlayer && HealthAIUtility.ShouldBeTendedNowByPlayer(Deliveree))
			{
				return JobCondition.Ongoing;
			}
			return ((job.playerForced || pawn.Faction != Faction.OfPlayer) && Deliveree.health.HasHediffsNeedingTend()) ? JobCondition.Ongoing : JobCondition.Succeeded;
		});
		this.FailOnAggroMentalState(TargetIndex.A);
		Toil reserveMedicine = null;
		Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, pathEndMode);
		if (usesMedicine)
		{
			List<Toil> list = CollectMedicineToils(pawn, Deliveree, job, gotoToil, out reserveMedicine);
			foreach (Toil item in list)
			{
				yield return item;
			}
		}
		yield return gotoToil;
		int ticks = (int)(1f / pawn.GetStatValue(StatDefOf.MedicalTendSpeed) * 600f);
		Toil waitToil;
		if (!job.draftedTend || pawn == base.TargetPawnA)
		{
			waitToil = Toils_General.Wait(ticks);
		}
		else
		{
			waitToil = Toils_General.WaitWith(TargetIndex.A, ticks, useProgressBar: false, maintainPosture: true, maintainSleep: false, TargetIndex.A, pathEndMode);
			waitToil.AddFinishAction(delegate
			{
				if (Deliveree != null && Deliveree != pawn && Deliveree.CurJob != null && (Deliveree.CurJob.def == JobDefOf.Wait || Deliveree.CurJob.def == JobDefOf.Wait_MaintainPosture))
				{
					Deliveree.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			});
		}
		waitToil.WithProgressBarToilDelay(TargetIndex.A).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
		waitToil.activeSkill = () => SkillDefOf.Medicine;
		waitToil.handlingFacing = true;
		waitToil.tickIntervalAction = delegate(int delta)
		{
			if (pawn == Deliveree && pawn.Faction != Faction.OfPlayer && pawn.IsHashIntervalTick(100, delta) && !pawn.Position.Fogged(pawn.Map))
			{
				FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
			}
			if (pawn != Deliveree)
			{
				pawn.rotationTracker.FaceTarget(Deliveree);
			}
		};
		waitToil.FailOn(() => pawn != Deliveree && !pawn.CanReachImmediate(Deliveree.SpawnedParentOrMe, pathEndMode));
		yield return Toils_Jump.JumpIf(waitToil, () => !usesMedicine || !IsMedicineInDoctorInventory);
		yield return Toils_Tend.PickupMedicine(TargetIndex.B, Deliveree).FailOnDestroyedOrNull(TargetIndex.B);
		yield return waitToil;
		yield return Toils_Tend.FinalizeTend(Deliveree);
		if (usesMedicine)
		{
			yield return FindMoreMedicineToil(pawn, Deliveree, TargetIndex.B, job, reserveMedicine);
		}
		yield return Toils_Jump.Jump(gotoToil);
	}

	public override void Notify_DamageTaken(DamageInfo dinfo)
	{
		base.Notify_DamageTaken(dinfo);
		if (dinfo.Def.ExternalViolenceFor(pawn) && pawn.Faction != Faction.OfPlayer && pawn == Deliveree)
		{
			pawn.jobs.CheckForJobOverride();
		}
	}

	public static List<Toil> CollectMedicineToils(Pawn doctor, Pawn patient, Job job, Toil gotoToil, out Toil reserveMedicine)
	{
		tmpCollectToils.Clear();
		Thing medicineUsed = job.targetB.Thing;
		Pawn_InventoryTracker medicineHolderInventory = medicineUsed?.ParentHolder as Pawn_InventoryTracker;
		Pawn otherPawnMedicineHolder = job.targetC.Pawn;
		reserveMedicine = Toils_Tend.ReserveMedicine(TargetIndex.B, patient).FailOnDespawnedNullOrForbidden(TargetIndex.B);
		tmpCollectToils.Add(Toils_Jump.JumpIf(gotoToil, () => medicineUsed != null && doctor.inventory.Contains(medicineUsed)));
		Toil toil = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch).FailOn(() => otherPawnMedicineHolder != medicineHolderInventory?.pawn || otherPawnMedicineHolder.IsForbidden(doctor));
		tmpCollectToils.Add(Toils_Haul.CheckItemCarriedByOtherPawn(medicineUsed, TargetIndex.C, toil));
		tmpCollectToils.Add(reserveMedicine);
		tmpCollectToils.Add(Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B));
		tmpCollectToils.Add(Toils_Tend.PickupMedicine(TargetIndex.B, patient).FailOnDestroyedOrNull(TargetIndex.B));
		tmpCollectToils.Add(Toils_Haul.CheckForGetOpportunityDuplicate(reserveMedicine, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true));
		tmpCollectToils.Add(Toils_Jump.Jump(gotoToil));
		tmpCollectToils.Add(toil);
		tmpCollectToils.Add(Toils_General.Wait(25).WithProgressBarToilDelay(TargetIndex.C));
		tmpCollectToils.Add(Toils_Haul.TakeFromOtherInventory(medicineUsed, doctor.inventory.innerContainer, medicineHolderInventory?.innerContainer, Medicine.GetMedicineCountToFullyHeal(patient), TargetIndex.B));
		return tmpCollectToils;
	}

	public static Toil FindMoreMedicineToil(Pawn doctor, Pawn patient, TargetIndex medicineIndex, Job job, Toil reserveMedicine)
	{
		Toil toil = ToilMaker.MakeToil("FindMoreMedicineToil");
		toil.initAction = delegate
		{
			if (job.GetTarget(medicineIndex).Thing.DestroyedOrNull())
			{
				Thing thing = HealthAIUtility.FindBestMedicine(doctor, patient);
				if (thing != null)
				{
					job.SetTarget(medicineIndex, thing);
					doctor.jobs.curDriver.JumpToToil(reserveMedicine);
				}
			}
		};
		return toil;
	}
}
