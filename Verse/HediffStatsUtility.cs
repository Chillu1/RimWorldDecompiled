using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;

namespace Verse
{
	public static class HediffStatsUtility
	{
		public static IEnumerable<StatDrawEntry> SpecialDisplayStats(HediffStage stage, Hediff instance)
		{
			if (instance != null && instance.Bleeding)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "BleedingRate".Translate(), instance.BleedRate.ToStringPercent() + "/" + "LetterDay".Translate(), "Stat_Hediff_BleedingRate_Desc".Translate(), 4040);
			}
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
				yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Pain".Translate(), (num * 100f).ToString("+###0;-###0") + "%", "Stat_Hediff_Pain_Desc".Translate(), 4050);
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
			if (stage != null && stage.partEfficiencyOffset != 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "PartEfficiency".Translate(), stage.partEfficiencyOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset), "Stat_Hediff_PartEfficiency_Desc".Translate(), 4050);
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
				for (int k = 0; k < capModsToDisplay.Count; k++)
				{
					PawnCapacityModifier capMod = capModsToDisplay[k];
					if (instance == null || instance.pawn == null || instance.pawn.health == null || instance.pawn.health.capacities.CapableOf(capMod.capacity))
					{
						if (capMod.offset != 0f)
						{
							yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, capMod.capacity.GetLabelFor(isFlesh: true, isHumanlike: true).CapitalizeFirst(), (capMod.offset * 100f).ToString("+#;-#") + "%", capMod.capacity.description, 4060);
						}
						if (capMod.postFactor != 1f)
						{
							yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, capMod.capacity.GetLabelFor(isFlesh: true, isHumanlike: true).CapitalizeFirst(), "x" + capMod.postFactor.ToStringPercent(), capMod.capacity.description, 4060);
						}
						if (capMod.SetMaxDefined && instance != null)
						{
							yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, capMod.capacity.GetLabelFor(isFlesh: true, isHumanlike: true).CapitalizeFirst(), "max".Translate().CapitalizeFirst() + " " + capMod.EvaluateSetMax(instance.pawn).ToStringPercent(), capMod.capacity.description, 4060);
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
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append("MemoryLower".Translate());
				}
				if (stage.AffectsSocialInteractions)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append("SocialInteractionsLower".Translate());
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
			if (stage.foodPoisoningChanceFactor != 1f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_Hediff_FoodPoisoningChanceFactor_Name".Translate(), stage.foodPoisoningChanceFactor.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Factor), "Stat_Hediff_FoodPoisoningChanceFactor_Desc".Translate(), 4030);
			}
			if (stage.statOffsets != null)
			{
				for (int k = 0; k < stage.statOffsets.Count; k++)
				{
					StatModifier statModifier = stage.statOffsets[k];
					if (statModifier.stat.CanShowWithLoadedMods())
					{
						yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifier.stat.LabelCap, statModifier.ValueToStringAsOffset, statModifier.stat.description, 4070);
					}
				}
			}
			if (stage.statFactors == null)
			{
				yield break;
			}
			for (int k = 0; k < stage.statFactors.Count; k++)
			{
				StatModifier statModifier2 = stage.statFactors[k];
				if (statModifier2.stat.CanShowWithLoadedMods())
				{
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifier2.stat.LabelCap, statModifier2.ToStringAsFactor, statModifier2.stat.description, 4070);
				}
			}
		}
	}
}
