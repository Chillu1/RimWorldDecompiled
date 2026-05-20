using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class HediffStage
{
	public float minSeverity;

	[MustTranslate]
	public string label;

	[MustTranslate]
	public string overrideLabel;

	[Unsaved(false)]
	[TranslationHandle]
	public string untranslatedLabel;

	public bool becomeVisible = true;

	public bool lifeThreatening;

	public TaleDef tale;

	public float vomitMtbDays = -1f;

	public float deathMtbDays = -1f;

	public bool mtbDeathDestroysBrain;

	public float painFactor = 1f;

	public float painOffset;

	public float totalBleedFactor = 1f;

	public float naturalHealingFactor = -1f;

	public float regeneration = -1f;

	public bool showRegenerationStat = true;

	public float forgetMemoryThoughtMtbDays = -1f;

	public float pctConditionalThoughtsNullified;

	public float pctAllThoughtNullification;

	public float opinionOfOthersFactor = 1f;

	public float fertilityFactor = 1f;

	public float hungerRateFactor = 1f;

	public float hungerRateFactorOffset;

	public float restFallFactor = 1f;

	public float restFallFactorOffset;

	public float socialFightChanceFactor = 1f;

	public float foodPoisoningChanceFactor = 1f;

	public float mentalBreakMtbDays = -1f;

	public string mentalBreakExplanation;

	public bool blocksMentalBreaks;

	public bool blocksInspirations;

	public float overrideMoodBase = -1f;

	public float severityGainFactor = 1f;

	public bool removeRoamMtb;

	public bool preventVacuumBurns;

	public List<MentalBreakIntensity> allowedMentalBreakIntensities;

	public List<HediffDef> makeImmuneTo;

	public List<PawnCapacityModifier> capMods = new List<PawnCapacityModifier>();

	public List<HediffGiver> hediffGivers;

	public List<MentalStateGiver> mentalStateGivers;

	public List<StatModifier> statOffsets;

	public List<StatModifier> statFactors;

	public List<StatModifierBySeverity> statOffsetsBySeverity;

	public List<StatModifierBySeverity> statFactorsBySeverity;

	public List<DamageFactor> damageFactors = new List<DamageFactor>();

	public List<NeedDef> enablesNeeds;

	public List<NeedDef> disablesNeeds;

	public bool multiplyStatChangesBySeverity;

	public StatDef statOffsetEffectMultiplier;

	public StatDef statFactorEffectMultiplier;

	public StatDef capacityFactorEffectMultiplier;

	public WorkTags disabledWorkTags;

	[MustTranslate]
	public string overrideTooltip;

	[MustTranslate]
	public string extraTooltip;

	public bool blocksSleeping;

	public float partEfficiencyOffset;

	public bool partIgnoreMissingHP;

	public bool destroyPart;

	public bool AffectsMemory
	{
		get
		{
			if (!(forgetMemoryThoughtMtbDays > 0f))
			{
				return pctConditionalThoughtsNullified > 0f;
			}
			return true;
		}
	}

	public bool AffectsSocialInteractions => opinionOfOthersFactor != 1f;

	public void PostLoad()
	{
		untranslatedLabel = label;
	}

	public IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		return HediffStatsUtility.SpecialDisplayStats(this, null);
	}
}
