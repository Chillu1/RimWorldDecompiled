using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class DifficultyDef : Def
	{
		public Color drawColor = Color.white;

		public bool isExtreme;

		public int difficulty = -1;

		public float threatScale = 1f;

		public bool allowBigThreats = true;

		public bool allowIntroThreats = true;

		public bool allowCaveHives = true;

		public bool peacefulTemples;

		public bool allowViolentQuests = true;

		public bool predatorsHuntHumanlikes = true;

		public float colonistMoodOffset;

		public float tradePriceFactorLoss;

		public float cropYieldFactor = 1f;

		public float mineYieldFactor = 1f;

		public float butcherYieldFactor = 1f;

		public float researchSpeedFactor = 1f;

		public float diseaseIntervalFactor = 1f;

		public float enemyReproductionRateFactor = 1f;

		public float playerPawnInfectionChanceFactor = 1f;

		public float manhunterChanceOnDamageFactor = 1f;

		public float deepDrillInfestationChanceFactor = 1f;

		public float foodPoisonChanceFactor = 1f;

		public float threatsGeneratorThreatCountFactor = 1f;

		public float maintenanceCostFactor = 1f;

		public float enemyDeathOnDownedChanceFactor = 1f;

		public float adaptationGrowthRateFactorOverZero = 1f;

		public float adaptationEffectFactor = 1f;

		public float questRewardValueFactor = 1f;
	}
}
