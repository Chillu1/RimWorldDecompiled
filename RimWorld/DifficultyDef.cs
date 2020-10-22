using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class DifficultyDef : Def
	{
		[Obsolete]
		public Color drawColor = Color.white;

		[Obsolete]
		public bool isExtreme;

		public bool isCustom;

		[Obsolete]
		public int difficulty = -1;

		public float threatScale = 1f;

		public bool allowBigThreats = true;

		public bool allowIntroThreats = true;

		public bool allowCaveHives = true;

		public bool peacefulTemples;

		public bool allowViolentQuests = true;

		public bool predatorsHuntHumanlikes = true;

		public float scariaRotChance;

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

		[Obsolete]
		public float threatsGeneratorThreatCountFactor = 1f;

		public float maintenanceCostFactor = 1f;

		public float enemyDeathOnDownedChanceFactor = 1f;

		public float adaptationGrowthRateFactorOverZero = 1f;

		public float adaptationEffectFactor = 1f;

		public float questRewardValueFactor = 1f;

		public float raidLootPointsFactor = 1f;

		public bool allowTraps = true;

		public bool allowTurrets = true;

		public bool allowMortars = true;

		public bool allowExtremeWeatherIncidents = true;

		public bool fixedWealthMode;
	}
}
