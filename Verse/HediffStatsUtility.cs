using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;

namespace Verse;

public static class HediffStatsUtility
{
	public static IEnumerable<StatDrawEntry> SpecialDisplayStats(HediffStage stage, Hediff instance)
	{
		if (instance != null && instance.Bleeding)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "BleedingRate".Translate(), instance.BleedRateScaled.ToStringPercent() + "/" + "LetterDay".Translate(), "Stat_Hediff_BleedingRate_Desc".Translate(), 4040);
		}
		if ((instance != null && instance.pawn.RaceProps.IsFlesh) || (stage != null && instance == null))
		{
			float num = 0f;
			if (instance != null)
			{
				num = instance.PainOffset;
			}
			else if (stage != null)
			{
				num = stage.painOffset;
			}
			if (num != 0f)
			{
				if (num > 0f && num < 0.01f)
				{
					num = 0.01f;
				}
				if (num < 0f && num > -0.01f)
				{
					num = -0.01f;
				}
				yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Pain".Translate(), $"{num * 100f:+###0;-###0}%", "Stat_Hediff_Pain_Desc".Translate(), 4050);
			}
			float num2 = 1f;
			if (instance != null)
			{
				num2 = instance.PainFactor;
			}
			else if (stage != null)
			{
				num2 = stage.painFactor;
			}
			if (num2 != 1f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Pain".Translate(), "x" + num2.ToStringPercent(), "Stat_Hediff_Pain_Desc".Translate(), 4050);
			}
		}
		if (stage != null && stage.partEfficiencyOffset != 0f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "PartEfficiency".Translate(), stage.partEfficiencyOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset), "Stat_Hediff_PartEfficiency_Desc".Translate(), 4050);
		}
		if (instance?.def.addedPartProps != null && instance.def.addedPartProps.partEfficiency != 1f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "PartEfficiency".Translate(), instance.def.addedPartProps.partEfficiency.ToStringByStyle(ToStringStyle.PercentZero), "Stat_Hediff_PartEfficiencyAbsolute_Desc".Translate(), 5000);
		}
		List<PawnCapacityModifier> capModsToDisplay = null;
		if (instance != null)
		{
			capModsToDisplay = instance.CapMods;
		}
		else if (stage != null)
		{
			capModsToDisplay = stage.capMods;
		}
		if (capModsToDisplay != null)
		{
			for (int i = 0; i < capModsToDisplay.Count; i++)
			{
				PawnCapacityModifier capMod = capModsToDisplay[i];
				if (instance?.pawn?.health == null || instance.pawn.health.capacities.CapableOf(capMod.capacity))
				{
					if (capMod.offset != 0f)
					{
						yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, capMod.capacity.GetLabelFor().CapitalizeFirst(), $"{capMod.offset * 100f:+#;-#}%", capMod.capacity.description, 4060);
					}
					if (capMod.postFactor != 1f)
					{
						yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, capMod.capacity.GetLabelFor().CapitalizeFirst(), "x" + capMod.postFactor.ToStringPercent(), capMod.capacity.description, 4060);
					}
					if (capMod.SetMaxDefined && instance != null)
					{
						yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, capMod.capacity.GetLabelFor().CapitalizeFirst(), "max".Translate().CapitalizeFirst() + " " + capMod.EvaluateSetMax(instance.pawn).ToStringPercent(), capMod.capacity.description, 4060);
					}
				}
			}
		}
		if (stage == null)
		{
			yield break;
		}
		if (stage.AffectsMemory || stage.AffectsSocialInteractions)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (stage.AffectsMemory)
			{
				string text = "MemoryLower".Translate();
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(", ");
				}
				else
				{
					text = text.CapitalizeFirst();
				}
				stringBuilder.Append(text);
			}
			if (stage.AffectsSocialInteractions)
			{
				string text2 = "SocialInteractionsLower".Translate();
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(", ");
				}
				else
				{
					text2 = text2.CapitalizeFirst();
				}
				stringBuilder.Append(text2);
			}
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Affects".Translate(), stringBuilder.ToString(), "Stat_Hediff_Affects_Desc".Translate(), 4080);
		}
		if (stage.hungerRateFactor != 1f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "HungerRate".Translate(), "x" + stage.hungerRateFactor.ToStringPercent(), "Stat_Hediff_HungerRateFactor_Desc".Translate(), 4051);
		}
		if (stage.hungerRateFactorOffset != 0f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Stat_Hediff_HungerRateOffset_Name".Translate(), stage.hungerRateFactorOffset.ToStringSign() + stage.hungerRateFactorOffset.ToStringPercent(), "Stat_Hediff_HungerRateOffset_Desc".Translate(), 4051);
		}
		if (stage.restFallFactor != 1f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Tiredness".Translate(), "x" + stage.restFallFactor.ToStringPercent(), "Stat_Hediff_TirednessFactor_Desc".Translate(), 4050);
		}
		if (stage.restFallFactorOffset != 0f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Stat_Hediff_TirednessOffset_Name".Translate(), stage.restFallFactorOffset.ToStringSign() + stage.restFallFactorOffset.ToStringPercent(), "Stat_Hediff_TirednessOffset_Desc".Translate(), 4050);
		}
		if (stage.makeImmuneTo != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "PreventsInfection".Translate(), stage.makeImmuneTo.Select((HediffDef im) => im.label).ToCommaList().CapitalizeFirst(), "Stat_Hediff_PreventsInfection_Desc".Translate(), 4050);
		}
		if (stage.totalBleedFactor != 1f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Stat_Hediff_TotalBleedFactor_Name".Translate(), stage.totalBleedFactor.ToStringPercent(), "Stat_Hediff_TotalBleedFactor_Desc".Translate(), 4041);
		}
		if (stage.naturalHealingFactor != -1f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Stat_Hediff_NaturalHealingFactor_Name".Translate(), stage.naturalHealingFactor.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Factor), "Stat_Hediff_NaturalHealingFactor_Desc".Translate(), 4020);
		}
		if (ModsConfig.AnomalyActive && stage.regeneration != -1f && stage.showRegenerationStat)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Stat_Hediff_Regeneration_Name".Translate(), "Stat_Hediff_Regeneration_Stat".Translate($"{stage.regeneration:0}"), "Stat_Hediff_Regeneration_Desc".Translate(), 4025);
		}
		if (stage.foodPoisoningChanceFactor != 1f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_Hediff_FoodPoisoningChanceFactor_Name".Translate(), stage.foodPoisoningChanceFactor.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Factor), "Stat_Hediff_FoodPoisoningChanceFactor_Desc".Translate(), 4030);
		}
		if (stage.statOffsets != null)
		{
			for (int i = 0; i < stage.statOffsets.Count; i++)
			{
				StatModifier statModifier = stage.statOffsets[i];
				if (statModifier.stat.CanShowWithLoadedMods())
				{
					float num3 = statModifier.value;
					if (instance != null && instance.CurStage.multiplyStatChangesBySeverity)
					{
						num3 *= instance.Severity;
					}
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifier.stat.LabelCap, statModifier.stat.Worker.ValueToString(num3, finalized: false, ToStringNumberSense.Offset), statModifier.stat.description, 4070);
				}
			}
		}
		if (stage.statOffsetsBySeverity != null && instance != null)
		{
			for (int i = 0; i < stage.statOffsetsBySeverity.Count; i++)
			{
				StatModifierBySeverity statModifierBySeverity = stage.statOffsetsBySeverity[i];
				if (statModifierBySeverity.stat.CanShowWithLoadedMods())
				{
					float val = statModifierBySeverity.valueBySeverity.Evaluate(instance.Severity);
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifierBySeverity.stat.LabelCap, statModifierBySeverity.stat.Worker.ValueToString(val, finalized: false, ToStringNumberSense.Offset), statModifierBySeverity.stat.description, 4070);
				}
			}
		}
		if (stage.statFactors != null)
		{
			for (int i = 0; i < stage.statFactors.Count; i++)
			{
				StatModifier statModifier2 = stage.statFactors[i];
				if (statModifier2.stat.CanShowWithLoadedMods())
				{
					float num4 = statModifier2.value;
					if (instance != null && instance.CurStage.multiplyStatChangesBySeverity)
					{
						num4 = StatWorker.ScaleFactor(num4, instance.Severity);
					}
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifier2.stat.LabelCap, statModifier2.stat.Worker.ValueToString(num4, finalized: false, ToStringNumberSense.Factor), statModifier2.stat.description, 4070);
				}
			}
		}
		if (stage.statFactorsBySeverity != null && instance != null)
		{
			for (int i = 0; i < stage.statFactorsBySeverity.Count; i++)
			{
				StatModifierBySeverity statModifierBySeverity2 = stage.statFactorsBySeverity[i];
				if (statModifierBySeverity2.stat.CanShowWithLoadedMods())
				{
					float val2 = statModifierBySeverity2.valueBySeverity.Evaluate(instance.Severity);
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifierBySeverity2.stat.LabelCap, statModifierBySeverity2.stat.Worker.ValueToString(val2, finalized: false, ToStringNumberSense.Factor), statModifierBySeverity2.stat.description, 4070);
				}
			}
		}
		if (stage.damageFactors.NullOrEmpty())
		{
			yield break;
		}
		for (int i = 0; i < stage.damageFactors.Count; i++)
		{
			DamageFactor damageFactor = stage.damageFactors[i];
			float num5 = damageFactor.factor;
			if (instance != null && instance.CurStage.multiplyStatChangesBySeverity)
			{
				num5 = StatWorker.ScaleFactor(num5, instance.Severity);
			}
			if (!(Math.Abs(num5 - 1f) < 0.001f))
			{
				yield return new StatDrawEntry(label: (!(num5 >= 1f)) ? ((string)"DamageFactorResistance".Translate(damageFactor.damageDef.Named("DAMAGE")).CapitalizeFirst()) : ((string)"DamageFactorWeakness".Translate(damageFactor.damageDef.Named("DAMAGE")).CapitalizeFirst()), category: StatCategoryDefOf.CapacityEffects, valueString: "x" + num5.ToStringPercent(), reportText: "DamageFactor".Translate(damageFactor.damageDef.Named("DAMAGE")).CapitalizeFirst(), displayPriorityWithinCategory: 4075);
			}
		}
	}

	public static float GetStatOffsetForSeverity(StatDef stat, HediffStage stage, Pawn pawn, float severity)
	{
		float num = stage.statOffsets.GetStatOffsetFromList(stat);
		if (num != 0f)
		{
			if (stage.statOffsetEffectMultiplier != null)
			{
				num *= pawn.GetStatValue(stage.statOffsetEffectMultiplier);
			}
			if (stage.multiplyStatChangesBySeverity)
			{
				num *= severity;
			}
			return num;
		}
		if (stage.statOffsetsBySeverity != null)
		{
			for (int i = 0; i < stage.statOffsetsBySeverity.Count; i++)
			{
				if (stage.statOffsetsBySeverity[i].stat == stat)
				{
					return stage.statOffsetsBySeverity[i].valueBySeverity.Evaluate(severity);
				}
			}
		}
		return 0f;
	}

	public static float GetStatFactorForSeverity(StatDef stat, HediffStage stage, Pawn pawn, float severity)
	{
		float num = stage.statFactors.GetStatFactorFromList(stat);
		if (Math.Abs(num - 1f) > float.Epsilon)
		{
			if (stage.statFactorEffectMultiplier != null)
			{
				num = StatWorker.ScaleFactor(num, pawn.GetStatValue(stage.statFactorEffectMultiplier));
			}
			if (stage.multiplyStatChangesBySeverity)
			{
				num = StatWorker.ScaleFactor(num, severity);
			}
			return num;
		}
		if (stage.statFactorsBySeverity != null)
		{
			for (int i = 0; i < stage.statFactorsBySeverity.Count; i++)
			{
				if (stage.statFactorsBySeverity[i].stat == stat)
				{
					return stage.statFactorsBySeverity[i].valueBySeverity.Evaluate(severity);
				}
			}
		}
		return 1f;
	}
}
