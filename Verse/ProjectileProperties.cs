using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse
{
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

		public SoundDef soundHitThickRoof;

		public SoundDef soundExplode;

		public SoundDef soundImpactAnticipate;

		public SoundDef soundAmbient;

		public float explosionRadius;

		public int explosionDelay;

		public ThingDef preExplosionSpawnThingDef;

		public float preExplosionSpawnChance = 1f;

		public int preExplosionSpawnThingCount = 1;

		public ThingDef postExplosionSpawnThingDef;

		public float postExplosionSpawnChance = 1f;

		public int postExplosionSpawnThingCount = 1;

		public bool applyDamageToExplosionCellsNeighbors;

		public float explosionChanceToStartFire;

		public bool explosionDamageFalloff;

		public EffecterDef explosionEffect;

		public bool ai_IsIncendiary;

		public float StoppingPower
		{
			get
			{
				if (stoppingPower != 0f)
				{
					return stoppingPower;
				}
				if (damageDef != null)
				{
					return damageDef.defaultStoppingPower;
				}
				return 0f;
			}
		}

		public float SpeedTilesPerTick => speed / 100f;

		public int GetDamageAmount(Thing weapon, StringBuilder explanation = null)
		{
			float weaponDamageMultiplier = weapon?.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier) ?? 1f;
			return GetDamageAmount(weaponDamageMultiplier, explanation);
		}

		public int GetDamageAmount_NewTmp(ThingDef weapon, ThingDef weaponStuff, StringBuilder explanation = null)
		{
			float weaponDamageMultiplier = weapon?.GetStatValueAbstract(StatDefOf.RangedWeapon_DamageMultiplier, weaponStuff) ?? 1f;
			return GetDamageAmount(weaponDamageMultiplier, explanation);
		}

		public int GetDamageAmount(float weaponDamageMultiplier, StringBuilder explanation = null)
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
			if (explanation != null)
			{
				explanation.AppendLine((string)("StatsReport_BaseValue".Translate() + ": ") + num);
				explanation.Append("StatsReport_QualityMultiplier".Translate() + ": " + weaponDamageMultiplier.ToStringPercent());
			}
			num = Mathf.RoundToInt((float)num * weaponDamageMultiplier);
			if (explanation != null)
			{
				explanation.AppendLine();
				explanation.AppendLine();
				explanation.Append((string)("StatsReport_FinalValue".Translate() + ": ") + num);
			}
			return num;
		}

		public float GetArmorPenetration(Thing weapon, StringBuilder explanation = null)
		{
			float weaponDamageMultiplier = weapon?.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier) ?? 1f;
			return GetArmorPenetration(weaponDamageMultiplier, explanation);
		}

		public float GetArmorPenetration(float weaponDamageMultiplier, StringBuilder explanation = null)
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
			if (explanation != null)
			{
				explanation.AppendLine("StatsReport_BaseValue".Translate() + ": " + num.ToStringPercent());
				explanation.AppendLine();
				explanation.Append("StatsReport_QualityMultiplier".Translate() + ": " + weaponDamageMultiplier.ToStringPercent());
			}
			num *= weaponDamageMultiplier;
			if (explanation != null)
			{
				explanation.AppendLine();
				explanation.AppendLine();
				explanation.Append("StatsReport_FinalValue".Translate() + ": " + num.ToStringPercent());
			}
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
		}
	}
}
