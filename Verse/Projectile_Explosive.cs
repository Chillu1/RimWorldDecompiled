namespace Verse
{
	public class Projectile_Explosive : Projectile
	{
		private int ticksToDetonation;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation", 0);
		}

		public override void Tick()
		{
			base.Tick();
			if (ticksToDetonation > 0)
			{
				ticksToDetonation--;
				if (ticksToDetonation <= 0)
				{
					Explode();
				}
			}
		}

		protected override void Impact(Thing hitThing)
		{
			if (def.projectile.explosionDelay == 0)
			{
				Explode();
				return;
			}
			landed = true;
			ticksToDetonation = def.projectile.explosionDelay;
			GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef, launcher.Faction);
		}

		protected virtual void Explode()
		{
			Map map = base.Map;
			Destroy();
			if (base.def.projectile.explosionEffect != null)
			{
				Effecter effecter = base.def.projectile.explosionEffect.Spawn();
				effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
				effecter.Cleanup();
			}
			GenExplosion.DoExplosion(base.Position, map, base.def.projectile.explosionRadius, base.def.projectile.damageDef, base.launcher, base.DamageAmount, base.ArmorPenetration, base.def.projectile.soundExplode, base.equipmentDef, base.def, intendedTarget.Thing, base.def.projectile.postExplosionSpawnThingDef, base.def.projectile.postExplosionSpawnChance, base.def.projectile.postExplosionSpawnThingCount, preExplosionSpawnThingDef: base.def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: base.def.projectile.preExplosionSpawnChance, preExplosionSpawnThingCount: base.def.projectile.preExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: base.def.projectile.applyDamageToExplosionCellsNeighbors, chanceToStartFire: base.def.projectile.explosionChanceToStartFire, damageFalloff: base.def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination));
		}
	}
}
