using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class AbilityDef : Def
{
	public Type abilityClass = typeof(Ability);

	public Type gizmoClass = typeof(Command_Ability);

	public List<AbilityCompProperties> comps = new List<AbilityCompProperties>();

	public AbilityCategoryDef category;

	public int displayOrder;

	public List<StatModifier> statBases;

	public VerbProperties verbProperties;

	public KeyBindingDef hotKey;

	public JobDef jobDef;

	public ThingDef warmupMote;

	public EffecterDef warmupEffecter;

	public FleckDef emittedFleck;

	public int emissionInterval;

	public string warmupMoteSocialSymbol;

	public SoundDef warmupStartSound;

	public SoundDef warmupSound;

	public SoundDef warmupPreEndSound;

	public float warmupPreEndSoundSeconds;

	public Vector3 moteDrawOffset;

	public float moteOffsetAmountTowardsTarget;

	public bool canUseAoeToGetTargets = true;

	public bool useAverageTargetPositionForWarmupEffecter;

	public bool targetRequired = true;

	public bool targetWorldCell;

	public bool showGizmoOnWorldView;

	public bool aiCanUse;

	public bool ai_SearchAOEForTargets;

	public bool ai_IsOffensive = true;

	public bool ai_IsIncendiary = true;

	public bool groupAbility;

	public int level;

	public IntRange cooldownTicksRange;

	public bool cooldownPerCharge;

	public bool hasExternallyHandledCooldown;

	public int charges = -1;

	public AbilityGroupDef groupDef;

	public bool overrideGroupCooldown;

	public List<MemeDef> requiredMemes;

	public bool sendLetterOnCooldownComplete;

	public bool sendMessageOnCooldownComplete;

	public bool displayGizmoWhileUndrafted;

	public bool disableGizmoWhileUndrafted = true;

	public bool writeCombatLog;

	public bool stunTargetWhileCasting;

	public bool showPsycastEffects = true;

	public bool showCastingProgressBar;

	public float detectionChanceOverride = -1f;

	public float uiOrder;

	public bool waitForJobEnd;

	public bool showWhenDrafted = true;

	public bool showOnCharacterCard = true;

	public bool hostile = true;

	public bool casterMustBeCapableOfViolence = true;

	[MustTranslate]
	public string confirmationDialogText;

	[NoTranslate]
	public string iconPath;

	public Texture2D uiIcon = BaseContent.BadTex;

	private string cachedTooltip;

	private Pawn cachedTooltipPawn;

	private List<string> cachedTargets;

	private int requiredPsyfocusBandCached = -1;

	private bool? anyCompOverridesPsyfocusCost;

	private FloatRange psyfocusCostRange = new FloatRange(-1f, -1f);

	private string psyfocusCostPercent;

	private string psyfocusCostPercentMax;

	private Texture2D warmupMoteSocialSymbolCached;

	public float EntropyGain => statBases.GetStatValueFromList(StatDefOf.Ability_EntropyGain, 0f);

	public float PsyfocusCost => statBases.GetStatValueFromList(StatDefOf.Ability_PsyfocusCost, 0f);

	public float EffectRadius => statBases.GetStatValueFromList(StatDefOf.Ability_EffectRadius, 0f);

	public bool HasAreaOfEffect => EffectRadius > float.Epsilon;

	public float DetectionChance
	{
		get
		{
			if (!(detectionChanceOverride >= 0f))
			{
				return this.GetStatValueAbstract(StatDefOf.Ability_DetectChancePerEntropy);
			}
			return detectionChanceOverride;
		}
	}

	public bool IsPsycast => typeof(Psycast).IsAssignableFrom(abilityClass);

	public string PsyfocusCostPercent
	{
		get
		{
			if (psyfocusCostPercent.NullOrEmpty())
			{
				psyfocusCostPercent = PsyfocusCost.ToStringPercent();
			}
			return psyfocusCostPercent;
		}
	}

	public string PsyfocusCostPercentMax
	{
		get
		{
			if (psyfocusCostPercentMax.NullOrEmpty())
			{
				psyfocusCostPercentMax = PsyfocusCostRange.max.ToStringPercent();
			}
			return psyfocusCostPercentMax;
		}
	}

	public int RequiredPsyfocusBand
	{
		get
		{
			if (requiredPsyfocusBandCached == -1)
			{
				requiredPsyfocusBandCached = Pawn_PsychicEntropyTracker.MaxAbilityLevelPerPsyfocusBand.Count - 1;
				for (int i = 0; i < Pawn_PsychicEntropyTracker.MaxAbilityLevelPerPsyfocusBand.Count; i++)
				{
					int num = Pawn_PsychicEntropyTracker.MaxAbilityLevelPerPsyfocusBand[i];
					if (level <= num)
					{
						requiredPsyfocusBandCached = i;
						break;
					}
				}
			}
			return requiredPsyfocusBandCached;
		}
	}

	public bool AnyCompOverridesPsyfocusCost
	{
		get
		{
			if (!anyCompOverridesPsyfocusCost.HasValue)
			{
				anyCompOverridesPsyfocusCost = false;
				if (comps != null)
				{
					foreach (AbilityCompProperties comp in comps)
					{
						if (comp.OverridesPsyfocusCost)
						{
							anyCompOverridesPsyfocusCost = true;
							break;
						}
					}
				}
			}
			return anyCompOverridesPsyfocusCost.Value;
		}
	}

	public FloatRange PsyfocusCostRange
	{
		get
		{
			if (psyfocusCostRange.min < 0f)
			{
				if (!AnyCompOverridesPsyfocusCost)
				{
					psyfocusCostRange = new FloatRange(PsyfocusCost, PsyfocusCost);
				}
				else
				{
					foreach (AbilityCompProperties comp in comps)
					{
						if (comp.OverridesPsyfocusCost)
						{
							psyfocusCostRange = comp.PsyfocusCostRange;
							break;
						}
					}
				}
			}
			return psyfocusCostRange;
		}
	}

	public Texture2D WarmupMoteSocialSymbol
	{
		get
		{
			if (!warmupMoteSocialSymbol.NullOrEmpty() && warmupMoteSocialSymbolCached == null)
			{
				warmupMoteSocialSymbolCached = ContentFinder<Texture2D>.Get(warmupMoteSocialSymbol);
			}
			return warmupMoteSocialSymbolCached;
		}
	}

	public IEnumerable<string> StatSummary(Pawn forPawn = null)
	{
		string text = null;
		foreach (AbilityCompProperties comp in comps)
		{
			if (comp.OverridesPsyfocusCost)
			{
				text = comp.PsyfocusCostExplanation;
				break;
			}
		}
		if (text == null)
		{
			if (PsyfocusCost > float.Epsilon)
			{
				yield return "AbilityPsyfocusCost".Translate() + ": " + PsyfocusCost.ToStringPercent();
			}
		}
		else
		{
			yield return text;
		}
		if (EntropyGain > float.Epsilon)
		{
			yield return string.Concat("AbilityEntropyGain".Translate() + ": ", EntropyGain.ToString());
		}
		if (verbProperties.warmupTime > float.Epsilon)
		{
			yield return string.Concat("AbilityCastingTime".Translate() + ": ", verbProperties.warmupTime.ToString()) + "LetterSecond".Translate();
		}
		if (cooldownTicksRange.min == cooldownTicksRange.max && cooldownTicksRange.min > 0)
		{
			yield return "StatsReport_Cooldown".Translate() + ": " + cooldownTicksRange.min.ToStringTicksToPeriod(allowSeconds: true, shortForm: false, canUseDecimals: true, allowYears: false);
		}
		float num = EffectDuration(forPawn);
		if (num > float.Epsilon)
		{
			int num2 = num.SecondsToTicks();
			yield return "AbilityDuration".Translate() + ": " + ((num2 >= 2500) ? num2.ToStringTicksToPeriod() : (num.ToString() + "LetterSecond".Translate()));
		}
		if (HasAreaOfEffect)
		{
			yield return string.Concat("AbilityEffectRadius".Translate() + ": ", Mathf.Ceil(EffectRadius).ToString());
		}
		if (comps == null)
		{
			yield break;
		}
		for (int i = 0; i < comps.Count; i++)
		{
			foreach (string item in comps[i].ExtraStatSummary())
			{
				yield return item;
			}
		}
	}

	public float EffectDuration(Pawn forPawn = null)
	{
		return this.GetStatValueAbstract(StatDefOf.Ability_Duration, forPawn);
	}

	public override void PostLoad()
	{
		if (!string.IsNullOrEmpty(iconPath))
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				uiIcon = ContentFinder<Texture2D>.Get(iconPath);
			});
		}
	}

	public string GetTooltip(Pawn pawn = null)
	{
		if (cachedTooltip == null || cachedTooltipPawn != pawn)
		{
			cachedTooltip = LabelCap.Colorize(ColoredText.TipSectionTitleColor) + ((level > 0) ? string.Concat("\n" + "Level".Translate().CapitalizeFirst() + " ", level.ToString()) : "") + "\n\n" + description;
			cachedTooltipPawn = pawn;
			string text = StatSummary(pawn).ToLineList();
			if (!text.NullOrEmpty())
			{
				cachedTooltip = cachedTooltip + "\n\n" + text;
			}
		}
		if (pawn != null && ModsConfig.RoyaltyActive && IsPsycast && level > 0)
		{
			Faction first = Faction.GetMinTitleForImplantAllFactions(HediffDefOf.PsychicAmplifier).First;
			if (first != null)
			{
				RoyalTitleDef minTitleForImplant = first.GetMinTitleForImplant(HediffDefOf.PsychicAmplifier, level);
				RoyalTitleDef currentTitle = pawn.royalty.GetCurrentTitle(first);
				if (minTitleForImplant != null && (currentTitle == null || currentTitle.seniority < minTitleForImplant.seniority) && DetectionChance > 0f)
				{
					return cachedTooltip + "\n\n" + ((string)"PsycastIsIllegal".Translate(pawn.Named("PAWN"), minTitleForImplant.GetLabelCapFor(pawn).Named("TITLE"))).Colorize(ColoredText.WarningColor);
				}
			}
		}
		return cachedTooltip;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		if (cachedTargets == null)
		{
			cachedTargets = new List<string>();
			if (verbProperties.targetParams.canTargetPawns && verbProperties.targetParams.canTargetSelf)
			{
				cachedTargets.Add("TargetSelf".Translate());
			}
			if (verbProperties.targetParams.canTargetLocations)
			{
				cachedTargets.Add("TargetGround".Translate());
			}
			if (verbProperties.targetParams.canTargetPawns && verbProperties.targetParams.canTargetHumans)
			{
				cachedTargets.Add("TargetHuman".Translate());
			}
			if (verbProperties.targetParams.canTargetPawns && verbProperties.targetParams.canTargetAnimals)
			{
				cachedTargets.Add("TargetAnimal".Translate());
			}
		}
		int num = comps.OfType<CompProperties_AbilityEffect>().Sum((CompProperties_AbilityEffect e) => e.goodwillImpact);
		if (num != 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Ability, StatDefOf.Ability_GoodwillImpact, num, req);
		}
		if (IsPsycast && level != 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Ability, StatDefOf.Ability_RequiredPsylink, level, req);
		}
		yield return new StatDrawEntry(StatCategoryDefOf.Ability, StatDefOf.Ability_CastingTime, verbProperties.warmupTime, req);
		if (verbProperties.range > 0f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Ability, StatDefOf.Ability_Range, verbProperties.range, req);
		}
		yield return new StatDrawEntry(StatCategoryDefOf.Ability, "Target".Translate(), cachedTargets.ToCommaList().CapitalizeFirst(), "AbilityTargetDesc".Translate(), 1001);
		yield return new StatDrawEntry(StatCategoryDefOf.Ability, "AbilityRequiresLOS".Translate(), verbProperties.requireLineOfSight ? "Yes".Translate() : "No".Translate(), "", 1000);
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (abilityClass == null)
		{
			yield return "abilityClass is null";
		}
		if (verbProperties == null)
		{
			yield return "verbProperties are null";
		}
		if (label.NullOrEmpty())
		{
			yield return "no label";
		}
		if (statBases != null)
		{
			foreach (StatModifier statBase in statBases)
			{
				if (statBases.Count((StatModifier st) => st.stat == statBase.stat) > 1)
				{
					yield return "defines the stat base " + statBase.stat?.ToString() + " more than once.";
				}
			}
		}
		for (int i = 0; i < comps.Count; i++)
		{
			foreach (string item2 in comps[i].ConfigErrors(this))
			{
				yield return item2;
			}
		}
	}
}
