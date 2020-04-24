using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobDriver_TakeToBed : JobDriver
	{
		private const TargetIndex TakeeIndex = TargetIndex.A;

		private const TargetIndex BedIndex = TargetIndex.B;

		protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		protected Building_Bed DropBed => (Building_Bed)job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
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
			AddFinishAction(delegate
			{
				if (job.def.makeTargetPrisoner && Takee.ownership.OwnedBed == DropBed && Takee.Position != RestUtility.GetBedSleepingSlotPosFor(Takee, DropBed))
				{
					Takee.ownership.UnclaimBed();
				}
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B)
				.FailOn(() => job.def == JobDefOf.Arrest && !Takee.CanBeArrestedBy(pawn))
				.FailOn(() => !pawn.CanReach(DropBed, PathEndMode.OnCell, Danger.Deadly))
				.FailOn(() => (job.def == JobDefOf.Rescue || job.def == JobDefOf.Capture) && !Takee.Downed)
				.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (job.def.makeTargetPrisoner)
				{
					Pawn pawn = (Pawn)job.targetA.Thing;
					pawn.GetLord()?.Notify_PawnAttemptArrested(pawn);
					GenClamor.DoClamor(pawn, 10f, ClamorDefOf.Harm);
					if (job.def == JobDefOf.Arrest && !pawn.CheckAcceptArrest(base.pawn))
					{
						base.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					if (!pawn.IsPrisoner)
					{
						QuestUtility.SendQuestTargetSignals(pawn.questTags, "Arrested", pawn.Named("SUBJECT"));
					}
				}
			};
			yield return toil;
			Toil toil2 = Toils_Haul.StartCarryThing(TargetIndex.A).FailOnNonMedicalBedNotOwned(TargetIndex.B, TargetIndex.A);
			toil2.AddPreInitAction(CheckMakeTakeeGuest);
			yield return toil2;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			Toil toil3 = new Toil();
			toil3.initAction = delegate
			{
				CheckMakeTakeePrisoner();
				if (Takee.playerSettings == null)
				{
					Takee.playerSettings = new Pawn_PlayerSettings(Takee);
				}
			};
			yield return toil3;
			yield return Toils_Reserve.Release(TargetIndex.B);
			Toil toil4 = new Toil();
			toil4.initAction = delegate
			{
				IntVec3 position = DropBed.Position;
				pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out Thing _);
				if (!DropBed.Destroyed && (DropBed.OwnersForReading.Contains(Takee) || (DropBed.Medical && DropBed.AnyUnoccupiedSleepingSlot) || Takee.ownership == null))
				{
					Takee.jobs.Notify_TuckedIntoBed(DropBed);
					if (Takee.RaceProps.Humanlike && job.def != JobDefOf.Arrest && !Takee.IsPrisonerOfColony)
					{
						Takee.relations.Notify_RescuedBy(pawn);
					}
					Takee.mindState.Notify_TuckedIntoBed();
				}
				if (Takee.IsPrisonerOfColony)
				{
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, Takee, OpportunityType.GoodToKnow);
				}
			};
			toil4.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil4;
		}

		private void CheckMakeTakeePrisoner()
		{
			if (job.def.makeTargetPrisoner)
			{
				if (Takee.guest.Released)
				{
					Takee.guest.Released = false;
					Takee.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
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
			if (!job.def.makeTargetPrisoner && Takee.Faction != Faction.OfPlayer && Takee.HostFaction != Faction.OfPlayer && Takee.guest != null && !Takee.IsWildMan())
			{
				Takee.guest.SetGuestStatus(Faction.OfPlayer);
			}
		}
	}
}
