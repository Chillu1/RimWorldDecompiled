namespace Verse;

public class Projectile_HellsphereCannon : Projectile
{
	private const float ExtraExplosionRadius = 4.9f;

	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
		Map map = base.Map;
		base.Impact(hitThing, blockedByShield);
		IntVec3 position = base.Position;
		DamageDef damageDef = base.DamageDef;
		Thing instigator = launcher;
		int damageAmount = DamageAmount;
		float armorPenetration = ArmorPenetration;
		ThingDef weapon = equipmentDef;
		ThingDef projectile = def;
		Thing thing = intendedTarget.Thing;
		float explosionChanceToStartFire = def.projectile.explosionChanceToStartFire;
		float expolosionPropagationSpeed = base.DamageDef.expolosionPropagationSpeed;
		GenExplosion.DoExplosion(position, map, 4.9f, damageDef, instigator, damageAmount, armorPenetration, null, weapon, projectile, thing, null, 0f, 1, null, null, 255, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, explosionChanceToStartFire, damageFalloff: false, null, null, null, doVisualEffects: true, expolosionPropagationSpeed);
	}
}
