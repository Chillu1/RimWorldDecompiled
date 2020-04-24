using Verse;

namespace RimWorld
{
	public class Projectile_DoomsdayRocket : Projectile
	{
		private const int ExtraExplosionCount = 3;

		private const int ExtraExplosionRadius = 5;

		protected override void Impact(Thing hitThing)
		{
			Map map = base.Map;
			base.Impact(hitThing);
			GenExplosion.DoExplosion(base.Position, map, base.def.projectile.explosionRadius, DamageDefOf.Bomb, base.launcher, base.DamageAmount, base.ArmorPenetration, null, base.equipmentDef, base.def, postExplosionSpawnThingDef: ThingDefOf.Filth_Fuel, intendedTarget: intendedTarget.Thing, postExplosionSpawnChance: 0.2f, postExplosionSpawnThingCount: 1, applyDamageToExplosionCellsNeighbors: false, preExplosionSpawnThingDef: null, preExplosionSpawnChance: 0f, preExplosionSpawnThingCount: 1, chanceToStartFire: 0.4f);
			CellRect cellRect = CellRect.CenteredOn(base.Position, 5);
			cellRect.ClipInsideMap(map);
			for (int i = 0; i < 3; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				DoFireExplosion(randomCell, map, 3.9f);
			}
		}

		protected void DoFireExplosion(IntVec3 pos, Map map, float radius)
		{
			GenExplosion.DoExplosion(pos, map, radius, DamageDefOf.Flame, launcher, base.DamageAmount, base.ArmorPenetration, null, equipmentDef, def, intendedTarget.Thing);
		}
	}
}
