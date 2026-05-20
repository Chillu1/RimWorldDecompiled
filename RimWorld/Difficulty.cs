using System.Text;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Difficulty : IExposable
{
	public const float DiseaseIntervalFactorCutoff = 100f;

	public const float FriendlyFireDefault = 0.4f;

	public const float AllowInstantKillChanceDefault = 1f;

	public const float MaintenanceCostFactorMin = 0.01f;

	public const float MinThreatPointsRangeCeilingDefault = 70f;

	[Unsaved(false)]
	private SimpleCurve minThreatPointsRangeCeilingCurveCached;

	public float threatScale = 1f;

	public bool allowBigThreats = true;

	public bool allowIntroThreats = true;

	public bool allowCaveHives = true;

	public bool peacefulTemples;

	public bool allowViolentQuests = true;

	public bool babiesAreHealthy;

	public bool noBabiesOrChildren;

	public bool childRaidersAllowed;

	public bool childShamblersAllowed = true;

	public bool predatorsHuntHumanlikes = true;

	public float scariaRotChance;

	public float colonistMoodOffset;

	public float tradePriceFactorLoss;

	public float cropYieldFactor = 1f;

	public float mineYieldFactor = 1f;

	public float butcherYieldFactor = 1f;

	public float fishingYieldFactor = 1f;

	public float researchSpeedFactor = 1f;

	public float diseaseIntervalFactor = 1f;

	public float enemyReproductionRateFactor = 1f;

	public float playerPawnInfectionChanceFactor = 1f;

	public float manhunterChanceOnDamageFactor = 1f;

	public float deepDrillInfestationChanceFactor = 1f;

	public float wastepackInfestationChanceFactor = 1f;

	public float foodPoisonChanceFactor = 1f;

	public float maintenanceCostFactor = 1f;

	public float enemyDeathOnDownedChanceFactor = 1f;

	public float adaptationGrowthRateFactorOverZero = 1f;

	public float adaptationEffectFactor = 1f;

	public float questRewardValueFactor = 1f;

	public float raidLootPointsFactor = 1f;

	public bool allowTraps = true;

	public bool allowTurrets = true;

	public bool allowMortars = true;

	public bool classicMortars;

	public bool allowExtremeWeatherIncidents = true;

	public bool fixedWealthMode;

	public float fixedWealthTimeFactor = 1f;

	public float friendlyFireChanceFactor = 0.4f;

	public float allowInstantKillChance = 1f;

	public float nomadicMineableResourcesFactor = 1f;

	public float lowPopConversionBoost = 3f;

	public float minThreatPointsRangeCeiling;

	public float childAgingRate = 4f;

	public float adultAgingRate = 1f;

	public bool unwaveringPrisoners = true;

	public float anomalyThreatsInactiveFraction = 0.08f;

	public float anomalyThreatsActiveFraction = 0.3f;

	public float? overrideAnomalyThreatsFraction;

	public float studyEfficiencyFactor = 1f;

	private AnomalyPlaystyleDef anomalyPlaystyleDef;

	public float MinThreatPointsCeiling => minThreatPointsRangeCeilingCurveCached.Evaluate(GenDate.DaysPassedSinceSettleFloat);

	public float EffectiveQuestRewardValueFactor => 1f / Mathf.Clamp(threatScale, 0.3f, 10f) * questRewardValueFactor;

	public float EffectiveRaidLootPointsFactor => 1f / Mathf.Clamp(threatScale, 0.3f, 10f) * raidLootPointsFactor;

	public bool ChildrenAllowed
	{
		get
		{
			if (ModsConfig.BiotechActive)
			{
				return !noBabiesOrChildren;
			}
			return false;
		}
	}

	public bool ChildRaidersAllowed
	{
		get
		{
			if (ChildrenAllowed)
			{
				return childRaidersAllowed;
			}
			return false;
		}
	}

	public AnomalyPlaystyleDef AnomalyPlaystyleDef
	{
		get
		{
			return anomalyPlaystyleDef ?? AnomalyPlaystyleDefOf.Standard;
		}
		set
		{
			anomalyPlaystyleDef = value;
		}
	}

	public Difficulty()
	{
	}

	public Difficulty(DifficultyDef src)
	{
		CopyFrom(src);
	}

	public bool AllowedToBuild(BuildableDef def)
	{
		if (def is ThingDef thingDef)
		{
			if (!allowTraps && thingDef.building.isTrap)
			{
				return false;
			}
			if ((!allowMortars || !allowTurrets) && thingDef.building.IsTurret)
			{
				if (!thingDef.building.IsMortar)
				{
					return allowTurrets;
				}
				return allowMortars;
			}
		}
		return true;
	}

	public bool AllowedBy(DifficultyConditionConfig cfg)
	{
		if (cfg == null)
		{
			return true;
		}
		if (!allowBigThreats && cfg.bigThreatsDisabled)
		{
			return false;
		}
		if (!allowTraps && cfg.trapsDisabled)
		{
			return false;
		}
		if (!allowTurrets && cfg.turretsDisabled)
		{
			return false;
		}
		if (!allowMortars && cfg.mortarsDisabled)
		{
			return false;
		}
		if (!allowExtremeWeatherIncidents && cfg.extremeWeatherIncidentsDisabled)
		{
			return false;
		}
		return true;
	}

	public float CertaintyReductionFactor(Pawn initiator, Pawn recipient)
	{
		if (initiator != null && initiator.Faction == Faction.OfPlayer && initiator.Ideo != null && initiator.Ideo.initialPlayerIdeo)
		{
			int num = ColonistsOfPlayerStartingIdeo();
			if (num > 2)
			{
				return 1f;
			}
			if (num == 2)
			{
				return lowPopConversionBoost / 2f;
			}
			return lowPopConversionBoost;
		}
		if (recipient != null && recipient.Faction == Faction.OfPlayer && recipient.Ideo != null && recipient.Ideo.initialPlayerIdeo)
		{
			int num2 = ColonistsOfPlayerStartingIdeo();
			if (num2 > 2)
			{
				return 1f;
			}
			if (num2 == 2)
			{
				return 1f / lowPopConversionBoost * 2f;
			}
			return 1f / lowPopConversionBoost;
		}
		return 1f;
		static int ColonistsOfPlayerStartingIdeo()
		{
			int num3 = 0;
			foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodger in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodgers)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodger.Ideo != null && allMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoLodger.Ideo.initialPlayerIdeo)
				{
					num3++;
				}
			}
			return num3;
		}
	}

	public void CopyFrom(DifficultyDef src)
	{
		threatScale = src.threatScale;
		allowBigThreats = src.allowBigThreats;
		allowIntroThreats = src.allowIntroThreats;
		allowCaveHives = src.allowCaveHives;
		peacefulTemples = src.peacefulTemples;
		allowViolentQuests = src.allowViolentQuests;
		predatorsHuntHumanlikes = src.predatorsHuntHumanlikes;
		scariaRotChance = src.scariaRotChance;
		colonistMoodOffset = src.colonistMoodOffset;
		tradePriceFactorLoss = src.tradePriceFactorLoss;
		cropYieldFactor = src.cropYieldFactor;
		mineYieldFactor = src.mineYieldFactor;
		butcherYieldFactor = src.butcherYieldFactor;
		fishingYieldFactor = src.fishingYieldFactor;
		researchSpeedFactor = src.researchSpeedFactor;
		diseaseIntervalFactor = src.diseaseIntervalFactor;
		enemyReproductionRateFactor = src.enemyReproductionRateFactor;
		playerPawnInfectionChanceFactor = src.playerPawnInfectionChanceFactor;
		manhunterChanceOnDamageFactor = src.manhunterChanceOnDamageFactor;
		deepDrillInfestationChanceFactor = src.deepDrillInfestationChanceFactor;
		wastepackInfestationChanceFactor = src.wastepackInfestationChanceFactor;
		nomadicMineableResourcesFactor = src.nomadicMineableResourcesFactor;
		foodPoisonChanceFactor = src.foodPoisonChanceFactor;
		maintenanceCostFactor = src.maintenanceCostFactor;
		enemyDeathOnDownedChanceFactor = src.enemyDeathOnDownedChanceFactor;
		adaptationGrowthRateFactorOverZero = src.adaptationGrowthRateFactorOverZero;
		adaptationEffectFactor = src.adaptationEffectFactor;
		questRewardValueFactor = src.questRewardValueFactor;
		raidLootPointsFactor = src.raidLootPointsFactor;
		allowTraps = src.allowTraps;
		allowTurrets = src.allowTurrets;
		allowMortars = src.allowMortars;
		classicMortars = src.classicMortars;
		allowExtremeWeatherIncidents = src.allowExtremeWeatherIncidents;
		fixedWealthMode = src.fixedWealthMode;
		fixedWealthTimeFactor = 1f;
		friendlyFireChanceFactor = 0.4f;
		allowInstantKillChance = 1f;
		lowPopConversionBoost = src.lowPopConversionBoost;
		minThreatPointsRangeCeiling = src.minThreatPointsRangeCeiling;
		babiesAreHealthy = src.babiesAreHealthy;
		noBabiesOrChildren = src.noBabiesOrChildren;
		childAgingRate = src.childAgingRate;
		adultAgingRate = src.adultAgingRate;
		unwaveringPrisoners = src.unwaveringPrisoners;
		childRaidersAllowed = src.childRaidersAllowed;
		anomalyThreatsInactiveFraction = src.anomalyThreatsInactiveFraction;
		anomalyThreatsActiveFraction = src.anomalyThreatsActiveFraction;
		studyEfficiencyFactor = src.studyEfficiencyFactor;
		SetMinThreatPointsCurve();
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref threatScale, "threatScale", 0f);
		Scribe_Values.Look(ref allowBigThreats, "allowBigThreats", defaultValue: false);
		Scribe_Values.Look(ref allowIntroThreats, "allowIntroThreats", defaultValue: false);
		Scribe_Values.Look(ref allowCaveHives, "allowCaveHives", defaultValue: false);
		Scribe_Values.Look(ref peacefulTemples, "peacefulTemples", defaultValue: false);
		Scribe_Values.Look(ref allowViolentQuests, "allowViolentQuests", defaultValue: false);
		Scribe_Values.Look(ref babiesAreHealthy, "babiesAreHealthy", defaultValue: false);
		Scribe_Values.Look(ref noBabiesOrChildren, "noBabiesOrChildren", defaultValue: false);
		Scribe_Values.Look(ref predatorsHuntHumanlikes, "predatorsHuntHumanlikes", defaultValue: false);
		Scribe_Values.Look(ref scariaRotChance, "scariaRotChance", 0f);
		Scribe_Values.Look(ref colonistMoodOffset, "colonistMoodOffset", 0f);
		Scribe_Values.Look(ref tradePriceFactorLoss, "tradePriceFactorLoss", 0f);
		Scribe_Values.Look(ref cropYieldFactor, "cropYieldFactor", 0f);
		Scribe_Values.Look(ref mineYieldFactor, "mineYieldFactor", 0f);
		Scribe_Values.Look(ref butcherYieldFactor, "butcherYieldFactor", 0f);
		Scribe_Values.Look(ref fishingYieldFactor, "fishingYieldFactor", 1f);
		Scribe_Values.Look(ref researchSpeedFactor, "researchSpeedFactor", 0f);
		Scribe_Values.Look(ref diseaseIntervalFactor, "diseaseIntervalFactor", 0f);
		Scribe_Values.Look(ref enemyReproductionRateFactor, "enemyReproductionRateFactor", 0f);
		Scribe_Values.Look(ref playerPawnInfectionChanceFactor, "playerPawnInfectionChanceFactor", 0f);
		Scribe_Values.Look(ref manhunterChanceOnDamageFactor, "manhunterChanceOnDamageFactor", 0f);
		Scribe_Values.Look(ref deepDrillInfestationChanceFactor, "deepDrillInfestationChanceFactor", 0f);
		Scribe_Values.Look(ref wastepackInfestationChanceFactor, "wastepackInfestationChanceFactor", 0f);
		Scribe_Values.Look(ref nomadicMineableResourcesFactor, "nomadicMineableResourcesFactor", 1f);
		Scribe_Values.Look(ref foodPoisonChanceFactor, "foodPoisonChanceFactor", 0f);
		Scribe_Values.Look(ref maintenanceCostFactor, "maintenanceCostFactor", 0f);
		Scribe_Values.Look(ref enemyDeathOnDownedChanceFactor, "enemyDeathOnDownedChanceFactor", 0f);
		Scribe_Values.Look(ref adaptationGrowthRateFactorOverZero, "adaptationGrowthRateFactorOverZero", 0f);
		Scribe_Values.Look(ref adaptationEffectFactor, "adaptationEffectFactor", 0f);
		Scribe_Values.Look(ref questRewardValueFactor, "questRewardValueFactor", 0f);
		Scribe_Values.Look(ref raidLootPointsFactor, "raidLootPointsFactor", 1f);
		Scribe_Values.Look(ref allowTraps, "allowTraps", defaultValue: true);
		Scribe_Values.Look(ref allowTurrets, "allowTurrets", defaultValue: true);
		Scribe_Values.Look(ref allowMortars, "allowMortars", defaultValue: true);
		Scribe_Values.Look(ref classicMortars, "classicMortars", defaultValue: true);
		Scribe_Values.Look(ref allowExtremeWeatherIncidents, "allowExtremeWeatherIncidents", defaultValue: true);
		Scribe_Values.Look(ref fixedWealthMode, "fixedWealthMode", defaultValue: false);
		Scribe_Values.Look(ref fixedWealthTimeFactor, "fixedWealthTimeFactor", 1f);
		Scribe_Values.Look(ref friendlyFireChanceFactor, "friendlyFireChanceFactor", 0.4f);
		Scribe_Values.Look(ref allowInstantKillChance, "allowInstantKillChance", 1f);
		Scribe_Values.Look(ref lowPopConversionBoost, "lowPopConversionBoost", 3f);
		Scribe_Values.Look(ref minThreatPointsRangeCeiling, "minThreatPointsRangeCeiling", 70f);
		Scribe_Values.Look(ref adultAgingRate, "adultAgingRate", 1f);
		Scribe_Values.Look(ref childAgingRate, "childAgingRate", 4f);
		Scribe_Values.Look(ref unwaveringPrisoners, "unwaveringPrisoners", defaultValue: false);
		Scribe_Values.Look(ref childRaidersAllowed, "childRaidersAllowed", defaultValue: true);
		Scribe_Values.Look(ref anomalyThreatsInactiveFraction, "anomalyThreatsInactiveFraction", 0.08f);
		Scribe_Values.Look(ref anomalyThreatsActiveFraction, "anomalyThreatsActiveFraction", 0.3f);
		Scribe_Values.Look(ref overrideAnomalyThreatsFraction, "overrideAnomalyThreatsFraction");
		Scribe_Values.Look(ref studyEfficiencyFactor, "studyEfficiencyFactor", 1f);
		Scribe_Defs.Look(ref anomalyPlaystyleDef, "anomalyPlaystyleDef");
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			maintenanceCostFactor = Mathf.Max(0.01f, maintenanceCostFactor);
			SetMinThreatPointsCurve();
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit && ModsConfig.AnomalyActive && anomalyPlaystyleDef == null)
		{
			anomalyPlaystyleDef = AnomalyPlaystyleDefOf.Standard;
		}
	}

	public string DebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("threatScale".PadRight(40)).Append(threatScale).AppendLine();
		stringBuilder.Append("allowBigThreats".PadRight(40)).Append(allowBigThreats).AppendLine();
		stringBuilder.Append("allowIntroThreats".PadRight(40)).Append(allowIntroThreats).AppendLine();
		stringBuilder.Append("allowCaveHives".PadRight(40)).Append(allowCaveHives).AppendLine();
		stringBuilder.Append("peacefulTemples".PadRight(40)).Append(peacefulTemples).AppendLine();
		stringBuilder.Append("allowViolentQuests".PadRight(40)).Append(allowViolentQuests).AppendLine();
		stringBuilder.Append("predatorsHuntHumanlikes".PadRight(40)).Append(predatorsHuntHumanlikes).AppendLine();
		stringBuilder.Append("scariaRotChance".PadRight(40)).Append(scariaRotChance).AppendLine();
		stringBuilder.Append("colonistMoodOffset".PadRight(40)).Append(colonistMoodOffset).AppendLine();
		stringBuilder.Append("tradePriceFactorLoss".PadRight(40)).Append(tradePriceFactorLoss).AppendLine();
		stringBuilder.Append("cropYieldFactor".PadRight(40)).Append(cropYieldFactor).AppendLine();
		stringBuilder.Append("mineYieldFactor".PadRight(40)).Append(mineYieldFactor).AppendLine();
		stringBuilder.Append("butcherYieldFactor".PadRight(40)).Append(butcherYieldFactor).AppendLine();
		stringBuilder.Append("fishingYieldFactor".PadRight(40)).Append(fishingYieldFactor).AppendLine();
		stringBuilder.Append("researchSpeedFactor".PadRight(40)).Append(researchSpeedFactor).AppendLine();
		stringBuilder.Append("diseaseIntervalFactor".PadRight(40)).Append(diseaseIntervalFactor).AppendLine();
		stringBuilder.Append("enemyReproductionRateFactor".PadRight(40)).Append(enemyReproductionRateFactor).AppendLine();
		stringBuilder.Append("playerPawnInfectionChanceFactor".PadRight(40)).Append(playerPawnInfectionChanceFactor).AppendLine();
		stringBuilder.Append("manhunterChanceOnDamageFactor".PadRight(40)).Append(manhunterChanceOnDamageFactor).AppendLine();
		stringBuilder.Append("deepDrillInfestationChanceFactor".PadRight(40)).Append(deepDrillInfestationChanceFactor).AppendLine();
		stringBuilder.Append("wastepackInfestationChanceFactor".PadRight(40)).Append(wastepackInfestationChanceFactor).AppendLine();
		stringBuilder.Append("nomadicMineableResourcesFactor".PadRight(40)).Append(nomadicMineableResourcesFactor).AppendLine();
		stringBuilder.Append("foodPoisonChanceFactor".PadRight(40)).Append(foodPoisonChanceFactor).AppendLine();
		stringBuilder.Append("maintenanceCostFactor".PadRight(40)).Append(maintenanceCostFactor).AppendLine();
		stringBuilder.Append("enemyDeathOnDownedChanceFactor".PadRight(40)).Append(enemyDeathOnDownedChanceFactor).AppendLine();
		stringBuilder.Append("adaptationGrowthRateFactorOverZero".PadRight(40)).Append(adaptationGrowthRateFactorOverZero).AppendLine();
		stringBuilder.Append("adaptationEffectFactor".PadRight(40)).Append(adaptationEffectFactor).AppendLine();
		stringBuilder.Append("questRewardValueFactor".PadRight(40)).Append(questRewardValueFactor).AppendLine();
		stringBuilder.Append("raidLootPointsFactor".PadRight(40)).Append(raidLootPointsFactor).AppendLine();
		stringBuilder.Append("allowTraps".PadRight(40)).Append(allowTraps).AppendLine();
		stringBuilder.Append("allowTurrets".PadRight(40)).Append(allowTurrets).AppendLine();
		stringBuilder.Append("allowMortars".PadRight(40)).Append(allowMortars).AppendLine();
		stringBuilder.Append("classicMortars".PadRight(40)).Append(classicMortars).AppendLine();
		stringBuilder.Append("allowExtremeWeatherIncidents".PadRight(40)).Append(allowExtremeWeatherIncidents).AppendLine();
		stringBuilder.Append("fixedWealthMode".PadRight(40)).Append(fixedWealthMode).AppendLine();
		stringBuilder.Append("fixedWealthTimeFactor".PadRight(40)).Append(fixedWealthTimeFactor).AppendLine();
		stringBuilder.Append("friendlyFireChanceFactor".PadRight(40)).Append(friendlyFireChanceFactor).AppendLine();
		stringBuilder.Append("allowInstantKillChance".PadRight(40)).Append(allowInstantKillChance).AppendLine();
		stringBuilder.Append("lowPopConversionBoost".PadRight(40)).Append(lowPopConversionBoost).AppendLine();
		stringBuilder.Append("minThreatPointsRangeCeiling".PadRight(40)).Append(minThreatPointsRangeCeiling).AppendLine();
		stringBuilder.Append("babiesAreHealthy".PadRight(40)).Append(babiesAreHealthy).AppendLine();
		stringBuilder.Append("noBabiesOrChildren".PadRight(40)).Append(noBabiesOrChildren).AppendLine();
		stringBuilder.Append("adultAgingRate".PadRight(40)).Append(adultAgingRate).AppendLine();
		stringBuilder.Append("childAgingRate".PadRight(40)).Append(childAgingRate).AppendLine();
		stringBuilder.Append("unwaveringPrisoners".PadRight(40)).Append(unwaveringPrisoners).AppendLine();
		stringBuilder.Append("childRaidersAllowed".PadRight(40)).Append(childRaidersAllowed).AppendLine();
		stringBuilder.Append("anomalyThreatsInactiveFraction".PadRight(40)).Append(anomalyThreatsInactiveFraction).AppendLine();
		stringBuilder.Append("anomalyThreatsActiveFraction".PadRight(40)).Append(anomalyThreatsActiveFraction).AppendLine();
		stringBuilder.Append("overrideAnomalyThreatsFraction".PadRight(40)).Append(overrideAnomalyThreatsFraction.HasValue ? overrideAnomalyThreatsFraction.ToString() : "null").AppendLine();
		stringBuilder.Append("studyEfficiencyFactor".PadRight(40)).Append(studyEfficiencyFactor).AppendLine();
		stringBuilder.Append("anomalyPlaystyleType".PadRight(40)).Append(anomalyPlaystyleDef.defName).AppendLine();
		return stringBuilder.ToString();
	}

	[DebugOutput]
	public static void DifficultyDetails()
	{
		Log.Message(Find.Storyteller.difficulty.DebugString());
	}

	private void SetMinThreatPointsCurve()
	{
		if (minThreatPointsRangeCeiling < 35f)
		{
			Log.Warning($"Min threat points range ceiling is below {35f}, resetting to {70f}");
			minThreatPointsRangeCeiling = 70f;
		}
		minThreatPointsRangeCeilingCurveCached = new SimpleCurve
		{
			new CurvePoint(12f, 35f),
			new CurvePoint(35f, minThreatPointsRangeCeiling)
		};
	}
}
