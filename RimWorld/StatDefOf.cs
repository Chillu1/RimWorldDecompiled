using System;

namespace RimWorld
{
	[DefOf]
	public static class StatDefOf
	{
		public static StatDef MaxHitPoints;

		public static StatDef MarketValue;

		public static StatDef MarketValueIgnoreHp;

		public static StatDef RoyalFavorValue;

		public static StatDef SellPriceFactor;

		public static StatDef Beauty;

		public static StatDef Cleanliness;

		public static StatDef Flammability;

		public static StatDef DeteriorationRate;

		public static StatDef WorkToMake;

		public static StatDef WorkToBuild;

		public static StatDef Mass;

		public static StatDef ConstructionSpeedFactor;

		public static StatDef Nutrition;

		public static StatDef FoodPoisonChanceFixedHuman;

		public static StatDef MoveSpeed;

		public static StatDef GlobalLearningFactor;

		public static StatDef HungerRateMultiplier;

		public static StatDef RestRateMultiplier;

		public static StatDef PsychicSensitivity;

		public static StatDef ToxicSensitivity;

		public static StatDef MentalBreakThreshold;

		public static StatDef EatingSpeed;

		public static StatDef ComfyTemperatureMin;

		public static StatDef ComfyTemperatureMax;

		public static StatDef Comfort;

		public static StatDef MeatAmount;

		public static StatDef LeatherAmount;

		public static StatDef MinimumHandlingSkill;

		public static StatDef MeleeDPS;

		public static StatDef PainShockThreshold;

		public static StatDef ForagedNutritionPerDay;

		[MayRequireRoyalty]
		public static StatDef PsychicEntropyMax;

		[MayRequireRoyalty]
		public static StatDef PsychicEntropyRecoveryRate;

		[MayRequireRoyalty]
		public static StatDef PsychicEntropyGain;

		[MayRequireRoyalty]
		public static StatDef MeditationFocusGain;

		public static StatDef WorkSpeedGlobal;

		public static StatDef MiningSpeed;

		public static StatDef DeepDrillingSpeed;

		public static StatDef MiningYield;

		public static StatDef ResearchSpeed;

		public static StatDef ConstructionSpeed;

		public static StatDef HuntingStealth;

		public static StatDef PlantWorkSpeed;

		public static StatDef SmoothingSpeed;

		public static StatDef FoodPoisonChance;

		public static StatDef CarryingCapacity;

		public static StatDef PlantHarvestYield;

		public static StatDef FixBrokenDownBuildingSuccessChance;

		public static StatDef ConstructSuccessChance;

		public static StatDef GeneralLaborSpeed;

		[DefAlias("GeneralLaborSpeed")]
		[Obsolete("Use StatDefOf.GeneralLaborSpeed, this field is only here for legacy reasons and will be removed in the future.")]
		public static StatDef UnskilledLaborSpeed;

		public static StatDef MedicalTendSpeed;

		public static StatDef MedicalTendQuality;

		public static StatDef MedicalSurgerySuccessChance;

		public static StatDef NegotiationAbility;

		public static StatDef ArrestSuccessChance;

		public static StatDef TradePriceImprovement;

		public static StatDef SocialImpact;

		public static StatDef PawnBeauty;

		public static StatDef AnimalGatherSpeed;

		public static StatDef AnimalGatherYield;

		public static StatDef TameAnimalChance;

		public static StatDef TrainAnimalChance;

		public static StatDef ShootingAccuracyPawn;

		public static StatDef ShootingAccuracyTurret;

		public static StatDef AimingDelayFactor;

		public static StatDef MeleeHitChance;

		public static StatDef MeleeDodgeChance;

		public static StatDef PawnTrapSpringChance;

		public static StatDef IncomingDamageFactor;

		public static StatDef MeleeWeapon_AverageDPS;

		public static StatDef MeleeWeapon_DamageMultiplier;

		public static StatDef MeleeWeapon_CooldownMultiplier;

		public static StatDef MeleeWeapon_AverageArmorPenetration;

		public static StatDef SharpDamageMultiplier;

		public static StatDef BluntDamageMultiplier;

		public static StatDef StuffPower_Armor_Sharp;

		public static StatDef StuffPower_Armor_Blunt;

		public static StatDef StuffPower_Armor_Heat;

		public static StatDef StuffPower_Insulation_Cold;

		public static StatDef StuffPower_Insulation_Heat;

		public static StatDef RangedWeapon_Cooldown;

		public static StatDef RangedWeapon_DamageMultiplier;

		public static StatDef AccuracyTouch;

		public static StatDef AccuracyShort;

		public static StatDef AccuracyMedium;

		public static StatDef AccuracyLong;

		public static StatDef StuffEffectMultiplierArmor;

		public static StatDef StuffEffectMultiplierInsulation_Cold;

		public static StatDef StuffEffectMultiplierInsulation_Heat;

		public static StatDef ArmorRating_Sharp;

		public static StatDef ArmorRating_Blunt;

		public static StatDef ArmorRating_Heat;

		public static StatDef Insulation_Cold;

		public static StatDef Insulation_Heat;

		public static StatDef EnergyShieldRechargeRate;

		public static StatDef EnergyShieldEnergyMax;

		public static StatDef SmokepopBeltRadius;

		[MayRequireRoyalty]
		public static StatDef JumpRange;

		public static StatDef EquipDelay;

		public static StatDef MedicalPotency;

		public static StatDef MedicalQualityMax;

		public static StatDef ImmunityGainSpeed;

		public static StatDef ImmunityGainSpeedFactor;

		public static StatDef DoorOpenSpeed;

		public static StatDef BedRestEffectiveness;

		public static StatDef TrapMeleeDamage;

		public static StatDef TrapSpringChance;

		public static StatDef ResearchSpeedFactor;

		public static StatDef MedicalTendQualityOffset;

		public static StatDef WorkTableWorkSpeedFactor;

		public static StatDef WorkTableEfficiencyFactor;

		public static StatDef JoyGainFactor;

		public static StatDef SurgerySuccessChanceFactor;

		public static StatDef Ability_CastingTime;

		public static StatDef Ability_EntropyGain;

		public static StatDef Ability_PsyfocusCost;

		public static StatDef Ability_Duration;

		public static StatDef Ability_Range;

		public static StatDef Ability_EffectRadius;

		public static StatDef Ability_RequiredPsylink;

		public static StatDef Ability_GoodwillImpact;

		public static StatDef Ability_DetectChancePerEntropy;

		[Obsolete("Will be removed in the future")]
		public static StatDef Bladelink_DetectionChance;

		public static StatDef MeditationFocusStrength;

		static StatDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf));
		}
	}
}
