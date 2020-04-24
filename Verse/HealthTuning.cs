namespace Verse
{
	public class HealthTuning
	{
		public struct PainCategoryWeighted
		{
			public PainCategory category;

			public float weight;

			public PainCategoryWeighted(PainCategory category, float weight)
			{
				this.category = category;
				this.weight = weight;
			}
		}

		public const int StandardInterval = 60;

		public const float SmallPawnFragmentedDamageHealthScaleThreshold = 0.5f;

		public const int SmallPawnFragmentedDamageMinimumDamageAmount = 10;

		public static float ChanceToAdditionallyDamageInnerSolidPart = 0.2f;

		public const float MinBleedingRateToBleed = 0.1f;

		public const float BleedSeverityRecoveryPerInterval = 0.00033333333f;

		public const float BloodFilthDropChanceFactorStanding = 0.004f;

		public const float BloodFilthDropChanceFactorLaying = 0.0004f;

		public const int BaseTicksAfterInjuryToStopBleeding = 90000;

		public const int TicksAfterMissingBodyPartToStopBeingFresh = 90000;

		public const float DefaultPainShockThreshold = 0.8f;

		public const int InjuryHealInterval = 600;

		public const float InjuryHealPerDay_Base = 8f;

		public const float InjuryHealPerDayOffset_Laying = 4f;

		public const float InjuryHealPerDayOffset_Tended = 8f;

		public const int InjurySeverityTendedPerMedicine = 20;

		public const float BaseTotalDamageLethalThreshold = 150f;

		public const float BecomePermanentBaseChance = 0.02f;

		public static readonly SimpleCurve BecomePermanentChanceFactorBySeverityCurve = new SimpleCurve
		{
			new CurvePoint(4f, 0f),
			new CurvePoint(14f, 1f)
		};

		public static readonly PainCategoryWeighted[] InjuryPainCategories = new PainCategoryWeighted[4]
		{
			new PainCategoryWeighted(PainCategory.Painless, 0.5f),
			new PainCategoryWeighted(PainCategory.LowPain, 0.2f),
			new PainCategoryWeighted(PainCategory.MediumPain, 0.2f),
			new PainCategoryWeighted(PainCategory.HighPain, 0.1f)
		};

		public const float MinDamagePartPctForInfection = 0.2f;

		public static readonly IntRange InfectionDelayRange = new IntRange(15000, 45000);

		public const float AnimalsInfectionChanceFactor = 0.1f;

		public const float HypothermiaGrowthPerDegreeUnder = 6.45E-05f;

		public const float HeatstrokeGrowthPerDegreeOver = 6.45E-05f;

		public const float MinHeatstrokeProgressPerInterval = 0.000375f;

		public const float MinHypothermiaProgress = 0.00075f;

		public const float HarmfulTemperatureOffset = 10f;

		public const float MinTempOverComfyMaxForBurn = 150f;

		public const float BurnDamagePerTempOverage = 0.06f;

		public const int MinBurnDamage = 3;

		public const float ImmunityGainRandomFactorMin = 0.8f;

		public const float ImmunityGainRandomFactorMax = 1.2f;

		public const float ImpossibleToFallSickIfAboveThisImmunityLevel = 0.6f;

		public const int HediffGiverUpdateInterval = 60;

		public const int VomitCheckInterval = 600;

		public const int DeathCheckInterval = 200;

		public const int ForgetRandomMemoryThoughtCheckInterval = 400;

		public const float PawnBaseHealthForSummary = 75f;

		public const float DeathOnDownedChance_NonColonyAnimal = 0.5f;

		public const float DeathOnDownedChance_NonColonyMechanoid = 1f;

		public static readonly SimpleCurve DeathOnDownedChance_NonColonyHumanlikeFromPopulationIntentCurve = new SimpleCurve
		{
			new CurvePoint(-1f, 0.92f),
			new CurvePoint(0f, 0.85f),
			new CurvePoint(1f, 0.62f),
			new CurvePoint(2f, 0.55f),
			new CurvePoint(8f, 0.3f)
		};

		public const float TendPriority_LifeThreateningDisease = 1f;

		public const float TendPriority_PerBleedRate = 1.5f;

		public const float TendPriority_DiseaseSeverityDecreasesWhenTended = 0.025f;
	}
}
