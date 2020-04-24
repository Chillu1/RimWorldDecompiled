using RimWorld;
using Verse.Sound;

namespace Verse
{
	public class Stance_Warmup : Stance_Busy
	{
		private Sustainer sustainer;

		private bool targetStartedDowned;

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
			if (verb != null && verb.verbProps.soundAiming != null)
			{
				SoundInfo info = SoundInfo.InMap(verb.caster, MaintenanceType.PerTick);
				if (verb.CasterIsPawn)
				{
					info.pitchFactor = 1f / verb.CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor);
				}
				sustainer = verb.verbProps.soundAiming.TrySpawnSustainer(info);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref targetStartedDowned, "targetStartDowned", defaultValue: false);
		}

		public override void StanceDraw()
		{
			if (Find.Selector.IsSelected(stanceTracker.pawn))
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

		protected override void Expire()
		{
			verb.WarmupComplete();
			base.Expire();
		}
	}
}
