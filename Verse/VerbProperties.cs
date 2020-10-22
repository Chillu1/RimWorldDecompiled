using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class VerbProperties
	{
		private enum RangeCategory : byte
		{
			Touch,
			Short,
			Medium,
			Long
		}

		public VerbCategory category = VerbCategory.Misc;

		[TranslationHandle]
		public Type verbClass = typeof(Verb);

		[MustTranslate]
		public string label;

		[Unsaved(false)]
		[TranslationHandle(Priority = 100)]
		public string untranslatedLabel;

		public bool isPrimary = true;

		public bool violent = true;

		public float minRange;

		public float range = 1.42f;

		public int burstShotCount = 1;

		public int ticksBetweenBurstShots = 15;

		public float noiseRadius = 3f;

		public bool hasStandardCommand;

		public bool targetable = true;

		public bool nonInterruptingSelfCast;

		public TargetingParameters targetParams = new TargetingParameters();

		public bool requireLineOfSight = true;

		public bool mustCastOnOpenGround;

		public bool forceNormalTimeSpeed = true;

		public bool onlyManualCast;

		public bool stopBurstWithoutLos = true;

		public SurpriseAttackProps surpriseAttack;

		public float commonality = 1f;

		public Intelligence minIntelligence;

		public float consumeFuelPerShot;

		public float warmupTime;

		public float defaultCooldownTime;

		public string commandIcon;

		public SoundDef soundCast;

		public SoundDef soundCastTail;

		public SoundDef soundAiming;

		public float muzzleFlashScale;

		public ThingDef impactMote;

		public bool drawAimPie = true;

		public EffecterDef warmupEffecter;

		public bool drawHighlightWithLineOfSight;

		public BodyPartGroupDef linkedBodyPartsGroup;

		public bool ensureLinkedBodyPartsGroupAlwaysUsable;

		public DamageDef meleeDamageDef;

		public int meleeDamageBaseAmount = 1;

		public float meleeArmorPenetrationBase = -1f;

		public bool ai_IsWeapon = true;

		public bool ai_IsBuildingDestroyer;

		public float ai_AvoidFriendlyFireRadius;

		public ThingDef defaultProjectile;

		public float forcedMissRadius;

		public float accuracyTouch = 1f;

		public float accuracyShort = 1f;

		public float accuracyMedium = 1f;

		public float accuracyLong = 1f;

		public ThingDef spawnDef;

		public TaleDef colonyWideTaleDef;

		public BodyPartTagDef bodypartTagTarget;

		public RulePackDef rangedFireRulepack;

		public const float DefaultArmorPenetrationPerDamage = 0.015f;

		private const float VerbSelectionWeightFactor_BodyPart = 0.3f;

		private const float MinLinkedBodyPartGroupEfficiencyIfMustBeAlwaysUsable = 0.4f;

		public bool CausesTimeSlowdown
		{
			get
			{
				if (ai_IsWeapon)
				{
					return forceNormalTimeSpeed;
				}
				return false;
			}
		}

		public bool LaunchesProjectile => typeof(Verb_LaunchProjectile).IsAssignableFrom(verbClass);

		public string AccuracySummaryString => accuracyTouch.ToStringPercent() + " - " + accuracyShort.ToStringPercent() + " - " + accuracyMedium.ToStringPercent() + " - " + accuracyLong.ToStringPercent();

		public bool IsMeleeAttack => typeof(Verb_MeleeAttack).IsAssignableFrom(verbClass);

		public bool CausesExplosion
		{
			get
			{
				if (defaultProjectile == null)
				{
					return false;
				}
				if (!typeof(Projectile_Explosive).IsAssignableFrom(defaultProjectile.thingClass) && !typeof(Projectile_DoomsdayRocket).IsAssignableFrom(defaultProjectile.thingClass))
				{
					return defaultProjectile.GetCompProperties<CompProperties_Explosive>() != null;
				}
				return true;
			}
		}

		public float AdjustedMeleeDamageAmount(Verb ownerVerb, Pawn attacker)
		{
			if (ownerVerb.verbProps != this)
			{
				Log.ErrorOnce("Tried to calculate melee damage amount for a verb with different verb props. verb=" + ownerVerb, 5469809);
				return 0f;
			}
			return AdjustedMeleeDamageAmount(ownerVerb.tool, attacker, ownerVerb.EquipmentSource, ownerVerb.HediffCompSource);
		}

		public float AdjustedMeleeDamageAmount(Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource)
		{
			if (!IsMeleeAttack)
			{
				Log.ErrorOnce($"Attempting to get melee damage for a non-melee verb {this}", 26181238);
			}
			float num = tool?.AdjustedBaseMeleeDamageAmount(equipment, meleeDamageDef) ?? ((float)meleeDamageBaseAmount);
			if (attacker != null)
			{
				num *= GetDamageFactorFor(tool, attacker, hediffCompSource);
			}
			return num;
		}

		public float AdjustedMeleeDamageAmount_NewTmp(Tool tool, Pawn attacker, ThingDef equipment, ThingDef equipmentStuff, HediffComp_VerbGiver hediffCompSource)
		{
			if (!IsMeleeAttack)
			{
				Log.ErrorOnce($"Attempting to get melee damage for a non-melee verb {this}", 26181238);
			}
			float num = tool?.AdjustedBaseMeleeDamageAmount_NewTmp(equipment, equipmentStuff, meleeDamageDef) ?? ((float)meleeDamageBaseAmount);
			if (attacker != null)
			{
				num *= GetDamageFactorFor(tool, attacker, hediffCompSource);
			}
			return num;
		}

		public float AdjustedArmorPenetration(Verb ownerVerb, Pawn attacker)
		{
			if (ownerVerb.verbProps != this)
			{
				Log.ErrorOnce("Tried to calculate armor penetration for a verb with different verb props. verb=" + ownerVerb, 9865767);
				return 0f;
			}
			return AdjustedArmorPenetration(ownerVerb.tool, attacker, ownerVerb.EquipmentSource, ownerVerb.HediffCompSource);
		}

		public float AdjustedArmorPenetration(Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource)
		{
			float num = tool?.armorPenetration ?? meleeArmorPenetrationBase;
			if (num < 0f)
			{
				num = AdjustedMeleeDamageAmount(tool, attacker, equipment, hediffCompSource) * 0.015f;
			}
			else if (equipment != null)
			{
				float statValue = equipment.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier);
				num *= statValue;
			}
			return num;
		}

		public float AdjustedArmorPenetration_NewTmp(Tool tool, Pawn attacker, ThingDef equipment, ThingDef equipmentStuff, HediffComp_VerbGiver hediffCompSource)
		{
			float num = tool?.armorPenetration ?? meleeArmorPenetrationBase;
			if (num < 0f)
			{
				num = AdjustedMeleeDamageAmount_NewTmp(tool, attacker, equipment, equipmentStuff, hediffCompSource) * 0.015f;
			}
			else if (equipment != null)
			{
				float statValueAbstract = equipment.GetStatValueAbstract(StatDefOf.MeleeWeapon_DamageMultiplier, equipmentStuff);
				num *= statValueAbstract;
			}
			return num;
		}

		private float AdjustedExpectedDamageForVerbUsableInMelee(Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource)
		{
			if (IsMeleeAttack)
			{
				return AdjustedMeleeDamageAmount(tool, attacker, equipment, hediffCompSource);
			}
			if (LaunchesProjectile && defaultProjectile != null)
			{
				return defaultProjectile.projectile.GetDamageAmount(equipment);
			}
			return 0f;
		}

		private float AdjustedExpectedDamageForVerbUsableInMelee_NewTmp(Tool tool, Pawn attacker, ThingDef equipment, ThingDef equipmentStuff, HediffComp_VerbGiver hediffCompSource)
		{
			if (IsMeleeAttack)
			{
				return AdjustedMeleeDamageAmount_NewTmp(tool, attacker, equipment, equipmentStuff, hediffCompSource);
			}
			if (LaunchesProjectile && defaultProjectile != null)
			{
				return defaultProjectile.projectile.GetDamageAmount_NewTmp(equipment, equipmentStuff);
			}
			return 0f;
		}

		public float AdjustedMeleeSelectionWeight(Verb ownerVerb, Pawn attacker)
		{
			if (ownerVerb.verbProps != this)
			{
				Log.ErrorOnce("Tried to calculate melee selection weight for a verb with different verb props. verb=" + ownerVerb, 385716351);
				return 0f;
			}
			return AdjustedMeleeSelectionWeight(ownerVerb.tool, attacker, ownerVerb.EquipmentSource, ownerVerb.HediffCompSource, ownerVerb.DirectOwner is Pawn);
		}

		public float AdjustedMeleeSelectionWeight(Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource, bool comesFromPawnNativeVerbs)
		{
			if (!IsMeleeAttack)
			{
				return 0f;
			}
			if (attacker != null && (int)attacker.RaceProps.intelligence < (int)minIntelligence)
			{
				return 0f;
			}
			float num = 1f;
			float num2 = AdjustedExpectedDamageForVerbUsableInMelee(tool, attacker, equipment, hediffCompSource);
			if (num2 >= 0.001f || !typeof(Verb_MeleeApplyHediff).IsAssignableFrom(verbClass))
			{
				num *= num2 * num2;
			}
			num *= commonality;
			if (tool != null)
			{
				num *= tool.chanceFactor;
			}
			if (comesFromPawnNativeVerbs && (tool == null || !tool.alwaysTreatAsWeapon))
			{
				num *= 0.3f;
			}
			return num;
		}

		public float AdjustedMeleeSelectionWeight_NewTmp(Tool tool, Pawn attacker, ThingDef equipment, ThingDef equipmentStuff, HediffComp_VerbGiver hediffCompSource, bool comesFromPawnNativeVerbs)
		{
			if (!IsMeleeAttack)
			{
				return 0f;
			}
			if (attacker != null && (int)attacker.RaceProps.intelligence < (int)minIntelligence)
			{
				return 0f;
			}
			float num = 1f;
			float num2 = AdjustedExpectedDamageForVerbUsableInMelee_NewTmp(tool, attacker, equipment, equipmentStuff, hediffCompSource);
			if (num2 >= 0.001f || !typeof(Verb_MeleeApplyHediff).IsAssignableFrom(verbClass))
			{
				num *= num2 * num2;
			}
			num *= commonality;
			if (tool != null)
			{
				num *= tool.chanceFactor;
			}
			if (comesFromPawnNativeVerbs && (tool == null || !tool.alwaysTreatAsWeapon))
			{
				num *= 0.3f;
			}
			return num;
		}

		public float AdjustedCooldown(Verb ownerVerb, Pawn attacker)
		{
			if (ownerVerb.verbProps != this)
			{
				Log.ErrorOnce("Tried to calculate cooldown for a verb with different verb props. verb=" + ownerVerb, 19485711);
				return 0f;
			}
			return AdjustedCooldown(ownerVerb.tool, attacker, ownerVerb.EquipmentSource);
		}

		public float AdjustedCooldown(Tool tool, Pawn attacker, Thing equipment)
		{
			if (tool != null)
			{
				return tool.AdjustedCooldown(equipment);
			}
			if (equipment != null && !IsMeleeAttack)
			{
				return equipment.GetStatValue(StatDefOf.RangedWeapon_Cooldown);
			}
			return defaultCooldownTime;
		}

		public float AdjustedCooldown_NewTmp(Tool tool, Pawn attacker, ThingDef equipment, ThingDef equipmentStuff)
		{
			if (tool != null)
			{
				return tool.AdjustedCooldown_NewTmp(equipment, equipmentStuff);
			}
			if (equipment != null && !IsMeleeAttack)
			{
				return equipment.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown, equipmentStuff);
			}
			return defaultCooldownTime;
		}

		public int AdjustedCooldownTicks(Verb ownerVerb, Pawn attacker)
		{
			return AdjustedCooldown(ownerVerb, attacker).SecondsToTicks();
		}

		private float AdjustedAccuracy(RangeCategory cat, Thing equipment)
		{
			if (equipment == null)
			{
				return cat switch
				{
					RangeCategory.Touch => accuracyTouch, 
					RangeCategory.Short => accuracyShort, 
					RangeCategory.Medium => accuracyMedium, 
					RangeCategory.Long => accuracyLong, 
					_ => throw new InvalidOperationException(), 
				};
			}
			StatDef stat = null;
			switch (cat)
			{
			case RangeCategory.Touch:
				stat = StatDefOf.AccuracyTouch;
				break;
			case RangeCategory.Short:
				stat = StatDefOf.AccuracyShort;
				break;
			case RangeCategory.Medium:
				stat = StatDefOf.AccuracyMedium;
				break;
			case RangeCategory.Long:
				stat = StatDefOf.AccuracyLong;
				break;
			}
			return equipment.GetStatValue(stat);
		}

		public float AdjustedFullCycleTime(Verb ownerVerb, Pawn attacker)
		{
			return warmupTime + AdjustedCooldown(ownerVerb, attacker) + ((burstShotCount - 1) * ticksBetweenBurstShots).TicksToSeconds();
		}

		public float GetDamageFactorFor(Verb ownerVerb, Pawn attacker)
		{
			if (ownerVerb.verbProps != this)
			{
				Log.ErrorOnce("Tried to calculate damage factor for a verb with different verb props. verb=" + ownerVerb, 94324562);
				return 1f;
			}
			return GetDamageFactorFor(ownerVerb.tool, attacker, ownerVerb.HediffCompSource);
		}

		public float GetDamageFactorFor(Tool tool, Pawn attacker, HediffComp_VerbGiver hediffCompSource)
		{
			float num = 1f;
			if (attacker != null)
			{
				if (hediffCompSource != null)
				{
					num *= PawnCapacityUtility.CalculatePartEfficiency(hediffCompSource.Pawn.health.hediffSet, hediffCompSource.parent.Part, ignoreAddedParts: true);
				}
				else if (attacker != null && AdjustedLinkedBodyPartsGroup(tool) != null)
				{
					float num2 = PawnCapacityUtility.CalculateNaturalPartsAverageEfficiency(attacker.health.hediffSet, AdjustedLinkedBodyPartsGroup(tool));
					if (AdjustedEnsureLinkedBodyPartsGroupAlwaysUsable(tool))
					{
						num2 = Mathf.Max(num2, 0.4f);
					}
					num *= num2;
				}
				if (attacker != null && IsMeleeAttack)
				{
					num *= attacker.ageTracker.CurLifeStage.meleeDamageFactor;
				}
			}
			return num;
		}

		public BodyPartGroupDef AdjustedLinkedBodyPartsGroup(Tool tool)
		{
			if (tool != null)
			{
				return tool.linkedBodyPartsGroup;
			}
			return linkedBodyPartsGroup;
		}

		public bool AdjustedEnsureLinkedBodyPartsGroupAlwaysUsable(Tool tool)
		{
			return tool?.ensureLinkedBodyPartsGroupAlwaysUsable ?? ensureLinkedBodyPartsGroupAlwaysUsable;
		}

		public float EffectiveMinRange(LocalTargetInfo target, Thing caster)
		{
			return EffectiveMinRange(VerbUtility.AllowAdjacentShot(target, caster));
		}

		public float EffectiveMinRange(bool allowAdjacentShot)
		{
			float num = minRange;
			if (!allowAdjacentShot && !IsMeleeAttack && LaunchesProjectile)
			{
				num = Mathf.Max(num, 1.421f);
			}
			return num;
		}

		public float GetHitChanceFactor(Thing equipment, float dist)
		{
			float value = ((dist <= 3f) ? AdjustedAccuracy(RangeCategory.Touch, equipment) : ((dist <= 12f) ? Mathf.Lerp(AdjustedAccuracy(RangeCategory.Touch, equipment), AdjustedAccuracy(RangeCategory.Short, equipment), (dist - 3f) / 9f) : ((dist <= 25f) ? Mathf.Lerp(AdjustedAccuracy(RangeCategory.Short, equipment), AdjustedAccuracy(RangeCategory.Medium, equipment), (dist - 12f) / 13f) : ((!(dist <= 40f)) ? AdjustedAccuracy(RangeCategory.Long, equipment) : Mathf.Lerp(AdjustedAccuracy(RangeCategory.Medium, equipment), AdjustedAccuracy(RangeCategory.Long, equipment), (dist - 25f) / 15f)))));
			return Mathf.Clamp(value, 0.01f, 1f);
		}

		public void DrawRadiusRing(IntVec3 center)
		{
			if (Find.CurrentMap == null || IsMeleeAttack || !targetable)
			{
				return;
			}
			float num = EffectiveMinRange(allowAdjacentShot: true);
			if (num > 0f && num < GenRadial.MaxRadialPatternRadius)
			{
				GenDraw.DrawRadiusRing(center, num);
			}
			if (!(range < (float)(Find.CurrentMap.Size.x + Find.CurrentMap.Size.z)) || !(range < GenRadial.MaxRadialPatternRadius))
			{
				return;
			}
			Func<IntVec3, bool> predicate = null;
			if (drawHighlightWithLineOfSight)
			{
				predicate = (IntVec3 c) => GenSight.LineOfSight(center, c, Find.CurrentMap);
			}
			GenDraw.DrawRadiusRing(center, range, Color.white, predicate);
		}

		public override string ToString()
		{
			string str = (label.NullOrEmpty() ? ("range=" + range + ", defaultProjectile=" + defaultProjectile.ToStringSafe()) : label);
			return "VerbProperties(" + str + ")";
		}

		public new VerbProperties MemberwiseClone()
		{
			return (VerbProperties)base.MemberwiseClone();
		}

		public IEnumerable<string> ConfigErrors(ThingDef parent)
		{
			if (parent.race != null && linkedBodyPartsGroup != null && !parent.race.body.AllParts.Any((BodyPartRecord part) => part.groups.Contains(linkedBodyPartsGroup)))
			{
				yield return string.Concat("has verb with linkedBodyPartsGroup ", linkedBodyPartsGroup, " but body ", parent.race.body, " has no parts with that group.");
			}
			if (LaunchesProjectile && defaultProjectile != null && forcedMissRadius > 0f != CausesExplosion)
			{
				yield return "has incorrect forcedMiss settings; explosive projectiles and only explosive projectiles should have forced miss enabled";
			}
		}

		public void PostLoad()
		{
			untranslatedLabel = label;
		}
	}
}
