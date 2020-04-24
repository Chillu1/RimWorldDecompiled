using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class Building_Turret : Building, IAttackTarget, ILoadReferenceable, IAttackTargetSearcher
	{
		protected StunHandler stunner;

		protected LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;

		private LocalTargetInfo lastAttackedTarget;

		private int lastAttackTargetTick;

		private const float SightRadiusTurret = 13.4f;

		public abstract LocalTargetInfo CurrentTarget
		{
			get;
		}

		public abstract Verb AttackVerb
		{
			get;
		}

		Thing IAttackTarget.Thing => this;

		public LocalTargetInfo TargetCurrentlyAimingAt => CurrentTarget;

		Thing IAttackTargetSearcher.Thing => this;

		public Verb CurrentEffectiveVerb => AttackVerb;

		public LocalTargetInfo LastAttackedTarget => lastAttackedTarget;

		public int LastAttackTargetTick => lastAttackTargetTick;

		public float TargetPriorityFactor => 1f;

		public Building_Turret()
		{
			stunner = new StunHandler(this);
		}

		public override void Tick()
		{
			base.Tick();
			if (forcedTarget.HasThing && (!forcedTarget.Thing.Spawned || !base.Spawned || forcedTarget.Thing.Map != base.Map))
			{
				forcedTarget = LocalTargetInfo.Invalid;
			}
			stunner.StunHandlerTick();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_TargetInfo.Look(ref forcedTarget, "forcedTarget");
			Scribe_TargetInfo.Look(ref lastAttackedTarget, "lastAttackedTarget");
			Scribe_Deep.Look(ref stunner, "stunner", this);
			Scribe_Values.Look(ref lastAttackTargetTick, "lastAttackTargetTick", 0);
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(ref dinfo, out absorbed);
			if (!absorbed)
			{
				stunner.Notify_DamageApplied(dinfo, affectedByEMP: true);
				absorbed = false;
			}
		}

		public abstract void OrderAttack(LocalTargetInfo targ);

		public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
		{
			CompPowerTrader comp = GetComp<CompPowerTrader>();
			if (comp != null && !comp.PowerOn)
			{
				return true;
			}
			CompMannable comp2 = GetComp<CompMannable>();
			if (comp2 != null && !comp2.MannedNow)
			{
				return true;
			}
			CompCanBeDormant comp3 = GetComp<CompCanBeDormant>();
			if (comp3 != null && !comp3.Awake)
			{
				return true;
			}
			CompInitiatable comp4 = GetComp<CompInitiatable>();
			if (comp4 != null && !comp4.Initiated)
			{
				return true;
			}
			return false;
		}

		protected void OnAttackedTarget(LocalTargetInfo target)
		{
			lastAttackTargetTick = Find.TickManager.TicksGame;
			lastAttackedTarget = target;
		}
	}
}
