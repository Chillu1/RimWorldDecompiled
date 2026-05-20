using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompDamageOnInterval : ThingComp
	{
		private int ticksToDamage;

		private Effecter effecter;

		public CompProperties_DamageOnInterval Props => (CompProperties_DamageOnInterval)props;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			ticksToDamage = Props.ticksBetweenDamage;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksToDamage, "ticksToPlantHarm", 0);
		}

		public override void CompTick()
		{
			if (!parent.Spawned || (Props.startHitPointsPercent > 0f && (float)parent.HitPoints > Props.startHitPointsPercent * (float)parent.MaxHitPoints))
			{
				return;
			}
			ticksToDamage--;
			if (ticksToDamage <= 0)
			{
				ticksToDamage = Props.ticksBetweenDamage;
				Damage();
			}
			EffecterDef effecterDef = null;
			foreach (DamageEffectStage effectStage in Props.effectStages)
			{
				if (effectStage.minHitPointsPercent * (float)parent.MaxHitPoints > (float)parent.HitPoints)
				{
					effecterDef = effectStage.effecterDef;
				}
			}
			if (effecterDef != null && effecterDef != effecter?.def)
			{
				effecter?.Cleanup();
				effecter = effecterDef.SpawnAttached(parent, parent.MapHeld);
			}
			effecter?.EffectTick(parent, parent);
		}

		public void Damage()
		{
			parent.TakeDamage(new DamageInfo(Props.damageDef ?? DamageDefOf.Deterioration, Props.damage));
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Damage";
				command_Action.action = Damage;
				yield return command_Action;
			}
		}
	}
}
