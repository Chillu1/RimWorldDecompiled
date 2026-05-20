using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public class ProjectileProperties
{
	public float speed = 5f;

	public bool flyOverhead;

	public bool alwaysFreeIntercept;

	public DamageDef damageDef;

	private int damageAmountBase = -1;

	private float armorPenetrationBase = -1f;

	public float stoppingPower = 0.5f;

	public List<ExtraDamage> extraDamages;

	public float arcHeightFactor;

	public float shadowSize;

	public EffecterDef landedEffecter;

	public float spinRate;

	public bool useGraphicClass;

	public SoundDef soundHitThickRoof;

	public SoundDef soundExplode;

	public SoundDef soundImpactAnticipate;

	public SoundDef soundAmbient;

	public SoundDef soundImpact;

	public float explosionRadius;

	public float explosionRadiusDisplayPadding;

	public int explosionDelay;

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

	public ThingDef filth;

	public float filthChance = 1f;

	public IntRange filthCount;

	public int numExtraHitCells;

	public bool explosionSpawnsSingleFilth;

	public TerrainDef spawnTerrain;

	public float terrainChance = 1f;

	public bool terrainReplacesFloors;

	public EffecterDef explosionEffect;

	public int explosionEffectLifetimeTicks;

	public ThingDef spawnsThingDef;

	public bool tryAdjacentFreeSpaces;

	public PawnKindDef spawnsPawnKind;

	public ThingDef beamMoteDef;

	public float beamStartOffset;

	public bool ai_IsIncendiary;

	public float SpeedTilesPerTick => speed / 100f;

	public int GetDamageAmount(Thing weapon, StringBuilder explanation = null)
	{
		float damageMultiplier = weapon?.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier) ?? 1f;
		return GetDamageAmount(damageMultiplier, weapon, explanation);
	}

	public int GetDamageAmount(ThingDef weapon, ThingDef weaponStuff, StringBuilder explanation = null)
	{
		float damageMultiplier = weapon?.GetStatValueAbstract(StatDefOf.RangedWeapon_DamageMultiplier, weaponStuff) ?? 1f;
		return GetDamageAmount(damageMultiplier, null, explanation);
	}

	public int GetDamageAmount(float damageMultiplier, Thing weapon, StringBuilder explanation = null)
	{
		int num = 0;
		if (damageAmountBase != -1)
		{
			num = damageAmountBase;
		}
		else
		{
			if (damageDef == null)
			{
				Log.ErrorOnce("Failed to find sane damage amount", 91094882);
				return 1;
			}
			num = damageDef.defaultDamage;
		}
		explanation?.AppendLine(string.Concat("StatsReport_BaseValue".Translate() + ": ", num.ToString()));
		num = Mathf.RoundToInt((float)num * damageMultiplier);
		if (!Mathf.Approximately(damageMultiplier, 1f))
		{
			explanation?.AppendLine();
			explanation?.AppendLine(StatDefOf.RangedWeapon_DamageMultiplier.LabelCap + ": " + damageMultiplier.ToStringPercent());
			if (weapon != null)
			{
				explanation?.Append(StatUtility.GetOffsetsAndFactorsFor(StatDefOf.RangedWeapon_DamageMultiplier, weapon));
			}
		}
		explanation?.AppendLine();
		explanation?.Append(string.Concat("StatsReport_FinalValue".Translate() + ": ", num.ToString()));
		return num;
	}

	public float GetArmorPenetration(Thing weapon = null, StringBuilder explanation = null)
	{
		if (damageDef.armorCategory == null)
		{
			return 0f;
		}
		float num;
		if (damageAmountBase != -1 || armorPenetrationBase >= 0f)
		{
			num = armorPenetrationBase;
		}
		else
		{
			if (damageDef == null)
			{
				return 0f;
			}
			num = damageDef.defaultArmorPenetration;
		}
		if (num < 0f)
		{
			num = (float)GetDamageAmount(null) * 0.015f;
		}
		explanation?.AppendLine("StatsReport_BaseValue".Translate() + ": " + num.ToStringPercent());
		if (weapon != null)
		{
			float statValue = weapon.GetStatValue(StatDefOf.RangedWeapon_ArmorPenetrationMultiplier);
			num *= statValue;
			if (!Mathf.Approximately(statValue, 1f))
			{
				explanation?.AppendLine();
				explanation?.AppendLine(StatDefOf.RangedWeapon_ArmorPenetrationMultiplier.LabelCap + ": " + statValue.ToStringPercent());
				explanation?.Append(StatUtility.GetOffsetsAndFactorsFor(StatDefOf.RangedWeapon_ArmorPenetrationMultiplier, weapon));
			}
		}
		explanation?.AppendLine();
		explanation?.AppendLine("StatsReport_FinalValue".Translate() + ": " + num.ToStringPercent());
		return num;
	}

	public IEnumerable<string> ConfigErrors(ThingDef parent)
	{
		if (alwaysFreeIntercept && flyOverhead)
		{
			yield return "alwaysFreeIntercept and flyOverhead are both true";
		}
		if (damageAmountBase == -1 && damageDef != null && damageDef.defaultDamage == -1)
		{
			yield return "no damage amount specified for projectile";
		}
		if (filth != null && filthCount.TrueMax <= 0)
		{
			yield return "has filth but filthCount <= 0.";
		}
		if (parent.thingClass == typeof(Projectile_SpawnsThing) && spawnsThingDef == null)
		{
			yield return "Projectile_SpawnsThing has no spawnsThingDef";
		}
	}
}
