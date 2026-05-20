using Verse;

namespace RimWorld;

public class CompProperties_AbilityExplosion : CompProperties_AbilityEffect
{
	public DamageDef damageDef;

	public int damageAmount = -1;

	public float armorPenetration = -1f;

	public SoundDef soundExplode;

	public float explosionRadius;

	public bool doExplosionVFX = true;

	public ThingDef preExplosionSpawnThingDef;

	public float preExplosionSpawnChance = 1f;

	public int preExplosionSpawnThingCount = 1;

	public ThingDef postExplosionSpawnThingDef;

	public ThingDef postExplosionSpawnThingDefWater;

	public float postExplosionSpawnChance = 1f;

	public int postExplosionSpawnThingCount = 1;

	public GasType? postExplosionGasType;

	public bool applyDamageToExplosionCellsNeighbors;

	public float explosionChanceToStartFire;

	public bool explosionDamageFalloff;

	public float screenShakeFactor = 1f;

	public ThingDef preExplosionSpawnSingleThingDef;

	public ThingDef postExplosionSpawnSingleThingDef;

	public CompProperties_AbilityExplosion()
	{
		compClass = typeof(CompAbilityEffect_Explosion);
	}
}
