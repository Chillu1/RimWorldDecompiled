using RimWorld;
using Verse.Sound;

namespace Verse
{
	public class Stance_Warmup : Stance_Busy
	{
		private Sustainer sustainer;

		private Effecter effecter;

		private bool targetStartedDowned;

		private bool drawAimPie = true;

		public Stance_Warmup()
		{
		}

		public Stance_Warmup(int ticks, LocalTargetInfo focusTarg, Verb verb)
			: base(ticks, focusTarg, verb)
		{
			if (focusTarg.HasThing && focusTarg.Thing is Pawn)
			{
				Pawn pawn = (Pawn)focusTarg.Thing;
				targetStartedDowned = pawn.Downed;
				if (pawn.apparel != null)
				{
					for (int i = 0; i < pawn.apparel.WornApparelCount; i++)
					{
						(pawn.apparel.WornApparel[i] as ShieldBelt)?.KeepDisplaying();
					}
				}
			}
			if (verb != null)
			{
				if (verb.verbProps.soundAiming != null)
				{
					SoundInfo info = SoundInfo.InMap(verb.caster, MaintenanceType.PerTick);
					if (verb.CasterIsPawn)
					{
						info.pitchFactor = 1f / verb.CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor);
					}
					sustainer = verb.verbProps.soundAiming.TrySpawnSustainer(info);
				}
				if (verb.verbProps.warmupEffecter != null && verb.Caster != null)
				{
					effecter = verb.verbProps.warmupEffecter.Spawn(verb.Caster, verb.Caster.Map);
					effecter.Trigger(verb.Caster, focusTarg.ToTargetInfo(verb.Caster.Map));
				}
			}
			drawAimPie = verb?.verbProps.drawAimPie ?? false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref targetStartedDowned, "targetStartDowned", defaultValue: false);
			Scribe_Values.Look(ref drawAimPie, "drawAimPie", defaultValue: false);
		}

		public override void StanceDraw()
		{
			if (drawAimPie && Find.Selector.IsSelected(stanceTracker.pawn))
			{
				GenDraw.DrawAimPie(stanceTracker.pawn, focusTarg, (int)((float)ticksLeft * pieSizeFactor), 0.2f);
			}
		}

		public override void StanceTick()
		{
			if (sustainer != null && !sustainer.Ended)
			{
				sustainer.Maintain();
			}
			effecter?.EffectTick(verb.Caster, focusTarg.ToTargetInfo(verb.Caster.Map));
			if (!targetStartedDowned && focusTarg.HasThing && focusTarg.Thing is Pawn && ((Pawn)focusTarg.Thing).Downed)
			{
				stanceTracker.SetStance(new Stance_Mobile());
				return;
			}
			if (focusTarg.HasThing && (!focusTarg.Thing.Spawned || verb == null || !verb.CanHitTargetFrom(base.Pawn.Position, focusTarg)))
			{
				stanceTracker.SetStance(new Stance_Mobile());
				return;
			}
			if (focusTarg == base.Pawn.mindState.enemyTarget)
			{
				base.Pawn.mindState.Notify_EngagedTarget();
			}
			base.StanceTick();
		}

		public void Interrupt()
		{
			base.Expire();
			effecter?.Cleanup();
		}

		protected override void Expire()
		{
			verb?.WarmupComplete();
			effecter?.Cleanup();
			base.Expire();
		}
	}
}
