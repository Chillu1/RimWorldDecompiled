using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Hunt : JobDriver
	{
		private int jobStartTick = -1;

		private const TargetIndex VictimInd = TargetIndex.A;

		private const TargetIndex CorpseInd = TargetIndex.A;

		private const TargetIndex StoreCellInd = TargetIndex.B;

		private const int MaxHuntTicks = 5000;

		public Pawn Victim
		{
			get
			{
				Corpse corpse = Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
				return (Pawn)job.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Corpse Corpse => job.GetTarget(TargetIndex.A).Thing as Corpse;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref jobStartTick, "jobStartTick", 0);
		}

		public override string GetReport()
		{
			if (Victim != null)
			{
				return JobUtility.GetResolvedJobReport(job.def.reportString, Victim);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(delegate
			{
				if (!job.ignoreDesignations)
				{
					Pawn victim2 = Victim;
					if (victim2 != null && !victim2.Dead && base.Map.designationManager.DesignationOn(victim2, DesignationDefOf.Hunt) == null)
					{
						return true;
					}
				}
				return false;
			});
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				jobStartTick = Find.TickManager.TicksGame;
			};
			yield return toil;
			yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
			Toil startCollectCorpseLabel = Toils_General.Label();
			Toil slaughterLabel = Toils_General.Label();
			Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, closeIfDowned: true, 0.95f).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpseLabel).FailOn(() => Find.TickManager.TicksGame > jobStartTick + 5000);
			yield return gotoCastPos;
			Toil slaughterIfPossible = Toils_Jump.JumpIf(slaughterLabel, delegate
			{
				Pawn victim = Victim;
				if (victim.RaceProps.DeathActionWorker != null && victim.RaceProps.DeathActionWorker.DangerousInMelee)
				{
					return false;
				}
				return victim.Downed ? true : false;
			});
			yield return slaughterIfPossible;
			yield return Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
			yield return Toils_Combat.CastVerb(TargetIndex.A, canHitNonTargetPawns: false).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpseLabel).FailOn(() => Find.TickManager.TicksGame > jobStartTick + 5000);
			yield return Toils_Jump.JumpIfTargetDespawnedOrNull(TargetIndex.A, startCollectCorpseLabel);
			yield return Toils_Jump.Jump(slaughterIfPossible);
			yield return slaughterLabel;
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnMobile(TargetIndex.A);
			yield return Toils_General.WaitWith(TargetIndex.A, 180, useProgressBar: true).FailOnMobile(TargetIndex.A);
			yield return Toils_General.Do(delegate
			{
				if (!Victim.Dead)
				{
					ExecutionUtility.DoExecutionByCut(pawn, Victim);
					pawn.records.Increment(RecordDefOf.AnimalsSlaughtered);
					if (pawn.InMentalState)
					{
						pawn.MentalState.Notify_SlaughteredAnimal();
					}
				}
			});
			yield return Toils_Jump.Jump(startCollectCorpseLabel);
			yield return startCollectCorpseLabel;
			yield return StartCollectCorpseToil();
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: true);
		}

		private Toil StartCollectCorpseToil()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (Victim == null)
				{
					toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				else
				{
					TaleRecorder.RecordTale(TaleDefOf.Hunted, pawn, Victim);
					Corpse corpse = Victim.Corpse;
					if (corpse == null || !pawn.CanReserveAndReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					else
					{
						corpse.SetForbidden(value: false);
						if (StoreUtility.TryFindBestBetterStoreCellFor(corpse, pawn, base.Map, StoragePriority.Unstored, pawn.Faction, out IntVec3 foundCell))
						{
							pawn.Reserve(corpse, job);
							pawn.Reserve(foundCell, job);
							job.SetTarget(TargetIndex.B, foundCell);
							job.SetTarget(TargetIndex.A, corpse);
							job.count = 1;
							job.haulMode = HaulMode.ToCellStorage;
						}
						else
						{
							pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
						}
					}
				}
			};
			return toil;
		}
	}
}
