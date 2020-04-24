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
			AddFinishAction(delegate
			{
				base.Map.attackTargetsCache.UpdateTarget(pawn);
			});
			Toil prepareToEatCorpse = new Toil();
			prepareToEatCorpse.initAction = delegate
			{
				Pawn actor = prepareToEatCorpse.actor;
				Corpse corpse = Corpse;
				if (corpse == null)
				{
					Pawn prey2 = Prey;
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
				base.Map.attackTargetsCache.UpdateTarget(pawn);
			});
			Action hitAction = delegate
			{
				Pawn prey = Prey;
				bool surpriseAttack = firstHit && !prey.IsColonist;
				if (pawn.meleeVerbs.TryMeleeAttack(prey, job.verbToUse, surpriseAttack))
				{
					if (!notifiedPlayerAttacked && PawnUtility.ShouldSendNotificationAbout(prey))
					{
						notifiedPlayerAttacked = true;
						Messages.Message("MessageAttackedByPredator".Translate(prey.LabelShort, pawn.LabelIndefinite(), prey.Named("PREY"), pawn.Named("PREDATOR")).CapitalizeFirst(), prey, MessageTypeDefOf.ThreatSmall);
					}
					base.Map.attackTargetsCache.UpdateTarget(pawn);
					firstHit = false;
				}
			};
			Toil toil = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction).JumpIfDespawnedOrNull(TargetIndex.A, prepareToEatCorpse).JumpIf(() => Corpse != null, prepareToEatCorpse)
				.FailOn(() => Find.TickManager.TicksGame > startTick + 5000 && (float)(job.GetTarget(TargetIndex.A).Cell - pawn.Position).LengthHorizontalSquared > 4f);
			toil.AddPreTickAction(CheckWarnPlayer);
			yield return toil;
			yield return prepareToEatCorpse;
			Toil gotoCorpse = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return gotoCorpse;
			float durationMultiplier = 1f / pawn.GetStatValue(StatDefOf.EatingSpeed);
			yield return Toils_Ingest.ChewIngestible(pawn, durationMultiplier, TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_Ingest.FinalizeIngest(pawn, TargetIndex.A);
			yield return Toils_Jump.JumpIf(gotoCorpse, () => pawn.needs.food.CurLevelPercentage < 0.9f);
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
