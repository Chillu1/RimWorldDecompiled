using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class AbilityDef : Def
	{
		public Type abilityClass = typeof(Ability);

		public Type gizmoClass = typeof(Command_Ability);

		public List<AbilityCompProperties> comps = new List<AbilityCompProperties>();

		public AbilityCategoryDef category;

		public List<StatModifier> statBases;

		public VerbProperties verbProperties;

		public KeyBindingDef hotKey;

		public JobDef jobDef;

		public ThingDef warmupMote;

		public SoundDef warmupStartSound;

		public SoundDef warmupSound;

		public SoundDef warmupPreEndSound;

		public float warmupPreEndSoundSeconds;

		public Vector3 moteDrawOffset;

		public float moteOffsetAmountTowardsTarget;

		public bool canUseAoeToGetTargets = true;

		public bool targetRequired = true;

		public bool targetWorldCell;

		public bool showGizmoOnWorldView;

		public int level;

		public IntRange cooldownTicksRange;

		public bool sendLetterOnCooldownComplete;

		public bool displayGizmoWhileUndrafted;

		public bool disableGizmoWhileUndrafted = true;

		public bool writeCombatLog;

		public bool stunTargetWhileCasting;

		public bool showPsycastEffects = true;

		public bool showCastingProgressBar;

		public float detectionChanceOverride = -1f;

		[MustTranslate]
		public string confirmationDialogText;

		[NoTranslate]
		public string iconPath;

		public Texture2D uiIcon = BaseContent.BadTex;

		private string cachedTooltip;

		private List<string> cachedTargets;

		private int requiredPsyfocusBandCached = -1;

		private bool? anyCompOverridesPsyfocusCost;

		private FloatRange psyfocusCostRange = new FloatRange(-1f, -1f);

		private string psyfocusCostPercent;

		private string psyfocusCostPercentMax;

		public float EntropyGain => statBases.GetStatValueFromList(StatDefOf.Ability_EntropyGain, 0f);

		public float PsyfocusCost => statBases.GetStatValueFromList(StatDefOf.Ability_PsyfocusCost, 0f);

		public float EffectRadius => statBases.GetStatValueFromList(StatDefOf.Ability_EffectRadius, 0f);

		public float EffectDuration => statBases.GetStatValueFromList(StatDefOf.Ability_Duration, 0f);

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

		public IEnumerable<string> StatSummary
		{
			get
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
					yield return (string)("AbilityEntropyGain".Translate() + ": ") + EntropyGain;
				}
				if (verbProperties.warmupTime > float.Epsilon)
				{
					yield return (string)("AbilityCastingTime".Translate() + ": ") + verbProperties.warmupTime + "LetterSecond".Translate();
				}
				float effectDuration = EffectDuration;
				if (effectDuration > float.Epsilon)
				{
					int num = effectDuration.SecondsToTicks();
					yield return "AbilityDuration".Translate() + ": " + ((num >= 2500) ? num.ToStringTicksToPeriod() : (effectDuration + (string)"LetterSecond".Translate()));
				}
				if (HasAreaOfEffect)
				{
					yield return (string)("AbilityEffectRadius".Translate() + ": ") + Mathf.Ceil(EffectRadius);
				}
			}
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
			if (cachedTooltip == null)
			{
				cachedTooltip = base.LabelCap + ((level > 0) ? ((string)("\n" + "Level".Translate() + " ") + level) : "") + "\n\n" + description;
				string text = StatSummary.ToLineList();
				if (!text.NullOrEmpty())
				{
					cachedTooltip = cachedTooltip + "\n\n" + text;
				}
			}
			if (pawn != null && ModsConfig.RoyaltyActive && abilityClass == typeof(Psycast) && level > 0)
			{
				Faction first = Faction.GetMinTitleForImplantAllFactions(HediffDefOf.PsychicAmplifier).First;
				if (first != null)
				{
					RoyalTitleDef minTitleForImplant = first.GetMinTitleForImplant(HediffDefOf.PsychicAmplifier, level);
					RoyalTitleDef currentTitle = pawn.royalty.GetCurrentTitle(first);
					if (minTitleForImplant != null && (currentTitle == null || currentTitle.seniority < minTitleForImplant.seniority) && DetectionChance > 0f)
					{
						return cachedTooltip + "\n\n" + ColoredText.Colorize("PsycastIsIllegal".Translate(pawn.Named("PAWN"), minTitleForImplant.GetLabelCapFor(pawn).Named("TITLE")), ColoredText.WarningColor);
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
			if (level != 0)
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
						yield return string.Concat("defines the stat base ", statBase.stat, " more than once.");
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
}
