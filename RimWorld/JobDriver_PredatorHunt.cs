using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PredatorHunt : JobDriver
	{
		private bool notifiedPlayerAttacked;

		private bool notifiedPlayerAttacking;

		private bool firstHit = true;

		public const TargetIndex PreyInd = TargetIndex.A;

		private const TargetIndex CorpseInd = TargetIndex.A;

		private const int MaxHuntTicks = 5000;

		public Pawn Prey
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
			Scribe_Values.Look(ref firstHit, "firstHit", defaultValue: false);
			Scribe_Values.Look(ref notifiedPlayerAttacking, "notifiedPlayerAttacking", defaultValue: false);
		}

		public override string GetReport()
		{
			if (Corpse != null)
			{
				return ReportStringProcessed(JobDefOf.Ingest.reportString);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			JobDriver_PredatorHunt jobDriver_PredatorHunt = this;
			AddFinishAction(delegate
			{
				jobDriver_PredatorHunt.Map.attackTargetsCache.UpdateTarget(jobDriver_PredatorHunt.pawn);
			});
			Toil prepareToEatCorpse = new Toil();
			prepareToEatCorpse.initAction = delegate
			{
				Pawn actor = prepareToEatCorpse.actor;
				Corpse corpse = jobDriver_PredatorHunt.Corpse;
				if (corpse == null)
				{
					Pawn prey2 = jobDriver_PredatorHunt.Prey;
					if (prey2 == null)
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable);
						return;
					}
					corpse = prey2.Corpse;
					if (corpse == null || !corpse.Spawned)
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable);
						return;
					}
				}
				if (actor.Faction == Faction.OfPlayer)
				{
					corpse.SetForbidden(value: false, warnOnFail: false);
				}
				else
				{
					corpse.SetForbidden(value: true, warnOnFail: false);
				}
				actor.CurJob.SetTarget(TargetIndex.A, corpse);
			};
			yield return Toils_General.DoAtomic(delegate
			{
				jobDriver_PredatorHunt.Map.attackTargetsCache.UpdateTarget(jobDriver_PredatorHunt.pawn);
			});
			Action hitAction = delegate
			{
				Pawn prey = jobDriver_PredatorHunt.Prey;
				bool surpriseAttack = jobDriver_PredatorHunt.firstHit && !prey.IsColonist;
				if (jobDriver_PredatorHunt.pawn.meleeVerbs.TryMeleeAttack(prey, jobDriver_PredatorHunt.job.verbToUse, surpriseAttack))
				{
					if (!jobDriver_PredatorHunt.notifiedPlayerAttacked && PawnUtility.ShouldSendNotificationAbout(prey))
					{
						jobDriver_PredatorHunt.notifiedPlayerAttacked = true;
						Messages.Message("MessageAttackedByPredator".Translate(prey.LabelShort, jobDriver_PredatorHunt.pawn.LabelIndefinite(), prey.Named("PREY"), jobDriver_PredatorHunt.pawn.Named("PREDATOR")).CapitalizeFirst(), prey, MessageTypeDefOf.ThreatSmall);
					}
					jobDriver_PredatorHunt.Map.attackTargetsCache.UpdateTarget(jobDriver_PredatorHunt.pawn);
					jobDriver_PredatorHunt.firstHit = false;
				}
			};
			Toil toil = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction).JumpIfDespawnedOrNull(TargetIndex.A, prepareToEatCorpse).JumpIf(() => jobDriver_PredatorHunt.Corpse != null, prepareToEatCorpse)
				.FailOn(() => Find.TickManager.TicksGame > jobDriver_PredatorHunt.startTick + 5000 && (float)(jobDriver_PredatorHunt.job.GetTarget(TargetIndex.A).Cell - jobDriver_PredatorHunt.pawn.Position).LengthHorizontalSquared > 4f);
			toil.AddPreTickAction(CheckWarnPlayer);
			yield return toil;
			yield return prepareToEatCorpse;
			Toil gotoCorpse = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return gotoCorpse;
			float durationMultiplier = 1f / pawn.GetStatValue(StatDefOf.EatingSpeed);
			yield return Toils_Ingest.ChewIngestible(pawn, durationMultiplier, TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_Ingest.FinalizeIngest(pawn, TargetIndex.A);
			yield return Toils_Jump.JumpIf(gotoCorpse, () => jobDriver_PredatorHunt.pawn.needs.food.CurLevelPercentage < 0.9f);
		}

		public override void Notify_DamageTaken(DamageInfo dinfo)
		{
			base.Notify_DamageTaken(dinfo);
			if (dinfo.Def.ExternalViolenceFor(pawn) && dinfo.Def.isRanged && dinfo.Instigator != null && dinfo.Instigator != Prey && !pawn.InMentalState && !pawn.Downed)
			{
				pawn.mindState.StartFleeingBecauseOfPawnAction(dinfo.Instigator);
			}
		}

		private void CheckWarnPlayer()
		{
			if (notifiedPlayerAttacking)
			{
				return;
			}
			Pawn prey = Prey;
			if (prey.Spawned && prey.Faction == Faction.OfPlayer && Find.TickManager.TicksGame > pawn.mindState.lastPredatorHuntingPlayerNotificationTick + 2500 && prey.Position.InHorDistOf(pawn.Position, 60f))
			{
				if (prey.RaceProps.Humanlike)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelPredatorHuntingColonist".Translate(pawn.LabelShort, prey.LabelDefinite(), pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), "LetterPredatorHuntingColonist".Translate(pawn.LabelIndefinite(), prey.LabelDefinite(), pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), LetterDefOf.ThreatBig, pawn);
				}
				else
				{
					Messages.Message((prey.Name.Numerical ? "LetterPredatorHuntingColonist" : "MessagePredatorHuntingPlayerAnimal").Translate(pawn.Named("PREDATOR"), prey.Named("PREY")), pawn, MessageTypeDefOf.ThreatBig);
				}
				pawn.mindState.Notify_PredatorHuntingPlayerNotification();
				notifiedPlayerAttacking = true;
			}
		}
	}
}
