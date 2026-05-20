using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualPatternDef : Def
{
	public RulePackDef nameMaker;

	public FloatRange ritualFreeStartIntervalDaysRange;

	public List<RitualObligationTriggerProperties> ritualObligationTriggers;

	public RitualObligationTargetFilterDef ritualObligationTargetFilter;

	public RitualTargetFilterDef ritualTargetFilter;

	public RitualBehaviorDef ritualBehavior;

	public RitualOutcomeEffectDef ritualOutcomeEffect;

	public bool ritualOnlyForIdeoMembers = true;

	public bool canStartAnytime;

	public bool alwaysStartAnytime;

	public bool allowOtherInstances;

	public bool playsIdeoMusic = true;

	public bool ignoreConsumableBuildingRequirement;

	public bool mergeGizmosForObligations;

	public bool canMergeGizmosFromDifferentIdeos = true;

	public bool ignoreExtremeTemperatures;

	public TechLevel minTechLevel;

	public TechLevel maxTechLevel;

	public bool showIdeoIconsInDialog = true;

	[MustTranslate]
	public string ritualExpectedDesc;

	[MustTranslate]
	public string ritualExpectedDescNoAdjective;

	[MustTranslate]
	public string shortDescOverride;

	[MustTranslate]
	public string descOverride;

	[MustTranslate]
	public string ritualExplanation;

	[MustTranslate]
	public string beginRitualOverride;

	[NoTranslate]
	public string iconPathOverride;

	[NoTranslate]
	public string cancelIconPathOverride;

	[NoTranslate]
	public List<string> tags;

	[NoTranslate]
	public string patternGroupTag;

	public List<PlanetLayerDef> layerWhitelist;

	public List<PlanetLayerDef> layerBlacklist;

	private Texture2D icon;

	public Texture2D Icon
	{
		get
		{
			if (icon == null)
			{
				if (iconPathOverride != null)
				{
					icon = ContentFinder<Texture2D>.Get(iconPathOverride);
				}
				else
				{
					icon = null;
				}
			}
			return icon;
		}
	}

	public void Fill(Precept_Ritual ritual)
	{
		ritual.nameMaker = nameMaker;
		if (!ritualObligationTriggers.NullOrEmpty())
		{
			ritual.obligationTriggers.Clear();
			foreach (RitualObligationTriggerProperties ritualObligationTrigger in ritualObligationTriggers)
			{
				RitualObligationTrigger instance = ritualObligationTrigger.GetInstance(ritual);
				ritual.obligationTriggers.Add(instance);
				instance.Init(ritualObligationTrigger);
			}
		}
		if (ritualOutcomeEffect != null)
		{
			ritual.outcomeEffect = ritualOutcomeEffect.GetInstance();
		}
		ritual.obligationTargetFilter = ritualObligationTargetFilter?.GetInstance();
		if (ritual.obligationTargetFilter != null)
		{
			ritual.obligationTargetFilter.parent = ritual;
		}
		ritual.targetFilter = ritualTargetFilter?.GetInstance();
		ritual.behavior = ritualBehavior.GetInstance();
		ritual.ritualOnlyForIdeoMembers = ritualOnlyForIdeoMembers;
		ritual.ritualExpectedDesc = ritualExpectedDesc;
		ritual.ritualExpectedDescNoAdjective = ritualExpectedDescNoAdjective;
		ritual.descOverride = descOverride;
		ritual.shortDescOverride = shortDescOverride;
		ritual.iconPathOverride = iconPathOverride;
		ritual.cancelIconPathOverride = cancelIconPathOverride;
		ritual.patternGroupTag = patternGroupTag;
		ritual.minTechLevel = minTechLevel;
		ritual.maxTechLevel = maxTechLevel;
		ritual.showIdeoIconsInDialog = showIdeoIconsInDialog;
		ritual.playsIdeoMusic = playsIdeoMusic;
		ritual.ritualExplanation = ritualExplanation;
		ritual.canBeAnytime = canStartAnytime;
		ritual.allowOtherInstances = allowOtherInstances;
		ritual.mergeGizmosForObligations = mergeGizmosForObligations;
		ritual.canMergeGizmosFromDifferentIdeos = canMergeGizmosFromDifferentIdeos;
		ritual.layerWhitelist = layerWhitelist;
		ritual.layerBlacklist = layerBlacklist;
		ritual.ignoreExtremeTemperatures = ignoreExtremeTemperatures;
		if (alwaysStartAnytime)
		{
			ritual.isAnytime = true;
		}
		else if (canStartAnytime)
		{
			Precept_Ritual precept_Ritual = ritual.ideo.PreceptsListForReading.OfType<Precept_Ritual>().FirstOrDefault((Precept_Ritual p) => p != ritual && p.behavior != null && p.behavior.def == ritualBehavior);
			int num = ritual.ideo.PreceptsListForReading.OfType<Precept_Ritual>().Count((Precept_Ritual r) => r != ritual && r.isAnytime && r.def.ritualPatternBase != null && r.def.ritualPatternBase.canStartAnytime);
			int num2 = ritual.ideo.PreceptsListForReading.OfType<Precept_Ritual>().Count((Precept_Ritual r) => r != ritual && !r.isAnytime && r.def.ritualPatternBase != null && r.def.ritualPatternBase.canStartAnytime);
			if (precept_Ritual != null)
			{
				ritual.isAnytime = false;
				precept_Ritual.isAnytime = false;
			}
			else if (num != num2)
			{
				ritual.isAnytime = num2 > num;
			}
			else
			{
				ritual.isAnytime = Rand.Bool;
			}
		}
		else
		{
			ritual.isAnytime = false;
		}
		if (ritual.SupportsAttachableOutcomeEffect)
		{
			ritual.attachableOutcomeEffect = DefDatabase<RitualAttachableOutcomeEffectDef>.AllDefs.Where((RitualAttachableOutcomeEffectDef d) => ValidateAttachedOutcome(d, allowDuplicates: false)).RandomElementWithFallback();
			if (ritual.attachableOutcomeEffect == null)
			{
				ritual.attachableOutcomeEffect = DefDatabase<RitualAttachableOutcomeEffectDef>.AllDefs.Where((RitualAttachableOutcomeEffectDef d) => ValidateAttachedOutcome(d, allowDuplicates: true)).RandomElementWithFallback();
			}
		}
		ritual.generatedAttachedReward = true;
		ritual.sourcePattern = this;
		bool ValidateAttachedOutcome(RitualAttachableOutcomeEffectDef def, bool allowDuplicates)
		{
			if (!def.CanAttachToRitual(ritual))
			{
				return false;
			}
			if (!allowDuplicates && ritual.ideo.PreceptsListForReading.OfType<Precept_Ritual>().Any((Precept_Ritual p) => p != ritual && p.attachableOutcomeEffect == def))
			{
				return false;
			}
			return true;
		}
	}

	public bool CanFactionUse(FactionDef faction)
	{
		if (faction == null || (Find.IdeoManager != null && Find.IdeoManager.classicMode))
		{
			return true;
		}
		return CanUseWithTechLevel(faction.techLevel, minTechLevel, maxTechLevel);
	}

	public static bool CanUseWithTechLevel(TechLevel level, TechLevel min, TechLevel max)
	{
		if ((int)min <= (int)level)
		{
			if (max != TechLevel.Undefined)
			{
				return (int)max >= (int)level;
			}
			return true;
		}
		return false;
	}
}
