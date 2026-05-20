using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_RevenantAttack : JobDriver
{
	private const TargetIndex VictimInd = TargetIndex.A;

	private const float NewTargetScanRadius = 20f;

	private const int SmearMTBTicks = 60;

	private const int CheckForCloserTargetMTB = 180;

	private static readonly int LongStunTicks = 2500;

	private int hypnotizeEndTick;

	private int HypnotizeDurationTicks => RevenantUtility.HypnotizeDurationSecondsFromNumColonistsCurve.Evaluate(RevenantUtility.NumSpawnedUnhypnotizedColonists(pawn.Map)).SecondsToTicks();

	private float RevealRange => RevenantUtility.RevealRangeFromNumColonistsCurve.Evaluate(RevenantUtility.NumSpawnedUnhypnotizedColonists(pawn.Map));

	private CompRevenant Comp => pawn.TryGetComp<CompRevenant>();

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref hypnotizeEndTick, "hypnotizeEndTick", 0);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		if (pawn.mindState.enemyTarget != null)
		{
			job.SetTarget(TargetIndex.A, pawn.mindState.enemyTarget);
		}
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		Toil stalk = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, TargetIndex.B, delegate
		{
			if (!base.pawn.stances.FullBodyBusy)
			{
				Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
				JobDriver curDriver = base.pawn.jobs.curDriver;
				Find.Anomaly.Hypnotize(pawn, base.pawn, LongStunTicks);
				curDriver.ReadyForNextToil();
			}
		});
		Toil toil = stalk;
		toil.initAction = (Action)Delegate.Combine(toil.initAction, (Action)delegate
		{
			pawn.Drawer.renderer.SetAnimation(AnimationDefOf.RevenantSpasm);
		});
		Toil toil2 = stalk;
		toil2.tickIntervalAction = (Action<int>)Delegate.Combine(toil2.tickIntervalAction, (Action<int>)delegate(int delta)
		{
			Job curJob = stalk.actor.jobs.curJob;
			if (Rand.MTBEventOccurs(180f, 1f, delta))
			{
				curJob.SetTarget(TargetIndex.A, RevenantUtility.GetClosestTargetInRadius(base.pawn, 10f) ?? curJob.GetTarget(TargetIndex.A).Pawn);
				base.pawn.mindState.enemyTarget = curJob.GetTarget(TargetIndex.A).Pawn;
			}
			if (curJob.targetA == null)
			{
				EndJobWith(JobCondition.Incompletable);
			}
			else
			{
				Pawn pawn = curJob.targetA.Pawn;
				if (base.pawn.Position.InHorDistOf(pawn.Position, RevealRange))
				{
					if (!Comp.Invisibility.PsychologicallyVisible)
					{
						Find.LetterStack.ReceiveLetter("LetterLabelRevenantRevealed".Translate(), Comp.everRevealed ? "LetterRevenantRevealed".Translate() : "LetterRevenantRevealedFirst".Translate(), LetterDefOf.ThreatBig, base.pawn, null, null, null, null, 60);
					}
					Comp.Invisibility.BecomeVisible();
					Find.TickManager.slower.SignalForceNormalSpeed();
				}
				if (Comp.Invisibility.PsychologicallyVisible && Rand.MTBEventOccurs(60f, 1f, delta))
				{
					RevenantUtility.CreateRevenantSmear(base.pawn);
				}
			}
		});
		stalk.AddFinishAction(delegate
		{
			Pawn pawn = job.GetTarget(TargetIndex.A).Pawn;
			if (pawn == null || !pawn.Spawned || pawn.Map != base.pawn.Map)
			{
				if (pawn != null)
				{
					Find.Anomaly.EndHypnotize(pawn);
				}
				job.SetTarget(TargetIndex.A, RevenantUtility.GetClosestTargetInRadius(base.pawn, 20f));
				base.pawn.mindState.enemyTarget = job.GetTarget(TargetIndex.A).Pawn;
				if (base.pawn.mindState.enemyTarget == null)
				{
					Comp.revenantState = RevenantState.Wander;
				}
			}
		});
		yield return stalk;
		Toil toil3 = ToilMaker.MakeToil("MakeNewToils");
		toil3.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		toil3.initAction = delegate
		{
			hypnotizeEndTick = Find.TickManager.TicksGame + HypnotizeDurationTicks;
		};
		toil3.AddFinishAction(delegate
		{
			Pawn pawn = job.GetTarget(TargetIndex.A).Pawn;
			Find.Anomaly.EndHypnotize(pawn);
			base.pawn.Drawer.renderer.SetAnimation(AnimationDefOf.RevenantSpasm);
		});
		toil3.tickAction = (Action)Delegate.Combine(toil3.tickAction, (Action)delegate
		{
			job.GetTarget(TargetIndex.A).Pawn.rotationTracker.FaceTarget(pawn);
			if (pawn.Drawer.renderer.CurAnimation != AnimationDefOf.RevenantHypnotise)
			{
				pawn.Drawer.renderer.SetAnimation(AnimationDefOf.RevenantHypnotise);
			}
			if (!pawn.stances.stunner.Stunned && Find.TickManager.TicksGame >= hypnotizeEndTick)
			{
				Pawn victim = job.GetTarget(TargetIndex.A).Pawn;
				Comp.Hypnotize(victim);
				if (Comp.injuredWhileAttacking)
				{
					Thing thing = ThingMaker.MakeThing(ThingDefOf.RevenantFleshChunk);
					thing.TryGetComp<CompAnalyzableBiosignature>().biosignature = Comp.biosignature;
					Thing thing2 = GenSpawn.Spawn(thing, pawn.PositionHeld, pawn.Map);
					Find.LetterStack.ReceiveLetter("LetterRevenantFleshChunkLabel".Translate(), "LetterRevenantFleshChunkText".Translate(), LetterDefOf.NeutralEvent, thing2, null, null, null, null, 600);
					Comp.injuredWhileAttacking = false;
				}
				Comp.revenantState = RevenantState.Escape;
				EndJobWith(JobCondition.Succeeded);
			}
		});
		toil3.defaultCompleteMode = ToilCompleteMode.Never;
		toil3.PlaySustainerOrSound(SoundDefOf.Pawn_Revenant_Hypnotize);
		yield return toil3;
	}

	public override bool IsContinuation(Job j)
	{
		return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
	}

	public override void Notify_PatherFailed()
	{
		Pawn pawn = job.GetTarget(TargetIndex.A).Pawn;
		if (pawn != null)
		{
			Find.Anomaly.EndHypnotize(pawn);
		}
		job.SetTarget(TargetIndex.A, RevenantUtility.GetClosestTargetInRadius(base.pawn, 20f));
		base.pawn.mindState.enemyTarget = job.GetTarget(TargetIndex.A).Pawn;
		if (base.pawn.mindState.enemyTarget == null)
		{
			Comp.revenantState = RevenantState.Wander;
		}
		base.Notify_PatherFailed();
	}
}
