using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobDriver_TakeToBed : JobDriver
{
	private const TargetIndex TakeeIndex = TargetIndex.A;

	private const TargetIndex BedIndex = TargetIndex.B;

	protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	protected Building_Bed DropBed => (Building_Bed)job.GetTarget(TargetIndex.B).Thing;

	private bool TakeeRescued
	{
		get
		{
			if (Takee.RaceProps.Humanlike && job.def != JobDefOf.Arrest && !Takee.IsPrisonerOfColony)
			{
				if (Takee.ageTracker.CurLifeStage.alwaysDowned)
				{
					return HealthAIUtility.ShouldSeekMedicalRest(Takee);
				}
				return true;
			}
			return false;
		}
	}

	public override string GetReport()
	{
		if (job.def == JobDefOf.Rescue && !TakeeRescued)
		{
			return "TakingToBed".Translate(Takee);
		}
		return base.GetReport();
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		Takee.ClearAllReservations();
		if (pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(DropBed, job, DropBed.SleepingSlotsCount, 0, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOnDestroyedOrNull(TargetIndex.B);
		this.FailOnAggroMentalStateAndHostile(TargetIndex.A);
		this.FailOn(delegate
		{
			if (job.def.makeTargetPrisoner)
			{
				if (!DropBed.ForPrisoners)
				{
					return true;
				}
			}
			else if (DropBed.ForPrisoners != Takee.IsPrisoner)
			{
				return true;
			}
			return false;
		});
		yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.B, TargetIndex.A);
		AddFinishAction(delegate(JobCondition jobCondition)
		{
			if (job.def.makeTargetPrisoner && Takee.ownership.OwnedBed == DropBed && Takee.Position != RestUtility.GetBedSleepingSlotPosFor(Takee, DropBed))
			{
				Takee.ownership.UnclaimBed();
			}
			if (jobCondition != JobCondition.Ongoing && pawn.carryTracker.CarriedThing != null)
			{
				pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Direct, out var _);
			}
		});
		Toil goToTakee = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B)
			.FailOn(() => job.def == JobDefOf.Arrest && !Takee.CanBeArrestedBy(pawn))
			.FailOn(() => !pawn.CanReach(DropBed, PathEndMode.OnCell, Danger.Deadly))
			.FailOn(() => (job.def == JobDefOf.Rescue || job.def == JobDefOf.Capture) && !Takee.Downed)
			.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		Toil checkArrestResistance = ToilMaker.MakeToil("MakeNewToils");
		checkArrestResistance.initAction = delegate
		{
			if (job.def.makeTargetPrisoner)
			{
				Pawn pawn = (Pawn)job.targetA.Thing;
				pawn.GetLord()?.Notify_PawnAttemptArrested(pawn);
				GenClamor.DoClamor(pawn, 10f, ClamorDefOf.Harm);
				if (!pawn.IsPrisoner && !pawn.IsSlave)
				{
					QuestUtility.SendQuestTargetSignals(pawn.questTags, "Arrested", pawn.Named("SUBJECT"));
					if (pawn.Faction != null)
					{
						QuestUtility.SendQuestTargetSignals(pawn.Faction.questTags, "FactionMemberArrested", pawn.Faction.Named("FACTION"));
					}
				}
				if (job.def == JobDefOf.Arrest && !pawn.CheckAcceptArrest(base.pawn))
				{
					base.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
			}
		};
		yield return Toils_Jump.JumpIf(checkArrestResistance, () => pawn.IsCarryingPawn(Takee));
		yield return goToTakee;
		yield return checkArrestResistance;
		Toil startCarrying = Toils_Haul.StartCarryThing(TargetIndex.A);
		startCarrying.FailOnBedNoLongerUsable(TargetIndex.B, TargetIndex.A);
		startCarrying.AddPreInitAction(CheckMakeTakeeGuest);
		startCarrying.AddFinishAction(delegate
		{
			if (pawn.Faction == Takee.Faction)
			{
				CheckMakeTakeePrisoner();
			}
		});
		Toil goToBed = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOn(() => !pawn.IsCarryingPawn(Takee));
		goToBed.FailOnBedNoLongerUsable(TargetIndex.B, TargetIndex.A);
		yield return Toils_Jump.JumpIf(goToBed, () => pawn.IsCarryingPawn(Takee));
		yield return startCarrying;
		yield return goToBed;
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			CheckMakeTakeePrisoner();
			if (Takee.playerSettings == null)
			{
				Takee.playerSettings = new Pawn_PlayerSettings(Takee);
			}
		};
		yield return toil;
		yield return Toils_Reserve.Release(TargetIndex.B);
		yield return Toils_Bed.TuckIntoBed(DropBed, pawn, Takee, TakeeRescued);
		yield return Toils_General.Do(delegate
		{
			if (!job.ritualTag.NullOrEmpty())
			{
				if (Takee.GetLord()?.LordJob is LordJob_Ritual lordJob_Ritual)
				{
					lordJob_Ritual.AddTagForPawn(Takee, job.ritualTag);
				}
				if (pawn.GetLord()?.LordJob is LordJob_Ritual lordJob_Ritual2)
				{
					lordJob_Ritual2.AddTagForPawn(pawn, job.ritualTag);
				}
			}
		});
	}

	private void CheckMakeTakeePrisoner()
	{
		if (job.def.makeTargetPrisoner)
		{
			if (Takee.guest.Released)
			{
				Takee.guest.Released = false;
				Takee.guest.SetExclusiveInteraction(PrisonerInteractionModeDefOf.MaintainOnly);
				GenGuest.RemoveHealthyPrisonerReleasedThoughts(Takee);
			}
			if (!Takee.IsPrisonerOfColony)
			{
				Takee.guest.CapturedBy(Faction.OfPlayer, pawn);
			}
		}
	}

	private void CheckMakeTakeeGuest()
	{
		if (!job.def.makeTargetPrisoner && Takee.Faction != Faction.OfPlayer && Takee.HostFaction != Faction.OfPlayer && Takee.guest != null && !Takee.IsWildMan() && Takee.DevelopmentalStage != DevelopmentalStage.Baby)
		{
			Takee.guest.SetGuestStatus(Faction.OfPlayer);
			QuestUtility.SendQuestTargetSignals(Takee.questTags, "Rescued", Takee.Named("SUBJECT"));
		}
	}
}
