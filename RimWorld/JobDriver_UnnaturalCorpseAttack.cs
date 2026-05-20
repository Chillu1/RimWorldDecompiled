using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_UnnaturalCorpseAttack : JobDriver
{
	private const TargetIndex VictimIndex = TargetIndex.A;

	private Pawn Victim => base.TargetPawnA;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		Find.Anomaly.TryGetUnnaturalCorpseTrackerForHaunted(Victim, out var tracker);
		int killTicks = tracker.TicksToKill;
		yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, TargetIndex.B, delegate
		{
			JobDriver curDriver = pawn.jobs.curDriver;
			int ticks = killTicks + 60;
			Find.Anomaly.Hypnotize(Victim, pawn, ticks);
			curDriver.ReadyForNextToil();
		});
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = (Action)Delegate.Combine(toil.initAction, (Action)delegate
		{
			Messages.Message("MessageAwokenAttacking".Translate(Victim.Named("PAWN")), Victim, MessageTypeDefOf.NegativeEvent);
			tracker.Notify_AwokenAttackStarting();
		});
		toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
		{
			job.GetTarget(TargetIndex.A).Pawn.rotationTracker.FaceTarget(pawn);
			Victim.health.GetOrAddHediff(HediffDefOf.AwokenHypnosis).TryGetComp<HediffComp_Disappears>().ticksToDisappear++;
			if (pawn.Drawer.renderer.CurAnimation != AnimationDefOf.UnnaturalCorpseAwokenKilling)
			{
				pawn.Drawer.renderer.SetAnimation(AnimationDefOf.UnnaturalCorpseAwokenKilling);
			}
		});
		toil.defaultDuration = killTicks;
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.PlaySustainerOrSound(SoundDefOf.Pawn_Awoken_Hypnotize);
		toil.AddFinishAction(delegate
		{
			if (pawn.Drawer.renderer.CurAnimation == AnimationDefOf.UnnaturalCorpseAwokenKilling)
			{
				pawn.Drawer.renderer.SetAnimation(null);
			}
		});
		yield return toil;
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.initAction = delegate
		{
			Pawn pawn = job.GetTarget(TargetIndex.A).Pawn;
			BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
			if (brain != null)
			{
				Find.BattleLog.Add(new BattleLogEntry_Event(pawn, RulePackDefOf.Event_UnnaturalCorpseAttack, base.pawn));
				DamageDef psychic = DamageDefOf.Psychic;
				BodyPartRecord hitPart = brain;
				pawn.TakeDamage(new DamageInfo(psychic, 99999f, 99999f, -1f, base.pawn, hitPart));
				Find.Anomaly.Notify_PawnKilledViaAwoken(pawn);
			}
		};
		toil2.PlaySustainerOrSound(SoundDefOf.CocoonDestroyed);
		yield return toil2;
	}

	public override bool IsContinuation(Job j)
	{
		return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
	}
}
