using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public class GeneDef : Def, IRenderNodePropertiesParent
{
	public Type geneClass = typeof(Gene);

	[MustTranslate]
	public string labelShortAdj;

	[MustTranslate]
	public List<string> customEffectDescriptions;

	[NoTranslate]
	public string iconPath;

	private Color? iconColor;

	public GeneCategoryDef displayCategory;

	public float displayOrderInCategory;

	public List<PawnRenderNodeProperties> renderNodeProperties;

	public bool neverGrayHair;

	public bool skinIsHairColor;

	public bool tattoosVisible = true;

	public FurDef fur;

	public SoundDef soundCall;

	public SoundDef soundDeath;

	public SoundDef soundWounded;

	public Type resourceGizmoType = typeof(GeneGizmo_Resource);

	public float resourceLossPerDay;

	[MustTranslate]
	public string resourceLabel;

	[MustTranslate]
	public string resourceDescription;

	public List<float> resourceGizmoThresholds;

	public bool showGizmoOnWorldView;

	public bool showGizmoWhenDrafted;

	public bool showGizmoOnMultiSelect;

	public List<AbilityDef> abilities;

	public List<GeneticTraitData> forcedTraits;

	public List<GeneticTraitData> suppressedTraits;

	public List<NeedDef> enablesNeeds;

	public List<NeedDef> disablesNeeds;

	public WorkTags disabledWorkTags;

	public bool ignoreDarkness;

	public EndogeneCategory endogeneCategory;

	public bool dislikesSunlight;

	public float minAgeActive;

	public float lovinMTBFactor = 1f;

	public bool immuneToToxGasExposure;

	public bool immuneToVacuumBurns;

	public bool randomChosen;

	public int? waterCellCost;

	public HistoryEventDef deathHistoryEvent;

	public List<Aptitude> aptitudes;

	public PassionMod passionMod;

	public List<StatModifier> statOffsets;

	public List<StatModifier> statFactors;

	public List<ConditionalStatAffecter> conditionalStatAffecters;

	public float painOffset;

	public float painFactor = 1f;

	public float foodPoisoningChanceFactor = 1f;

	public List<DamageFactor> damageFactors;

	public SimpleCurve biologicalAgeTickFactorFromAgeCurve;

	public List<HediffDef> makeImmuneTo;

	public List<HediffDef> hediffGiversCannotGive;

	public ChemicalDef chemical;

	public float addictionChanceFactor = 1f;

	public float overdoseChanceFactor = 1f;

	public float toleranceBuildupFactor = 1f;

	public bool sterilize;

	public List<PawnCapacityModifier> capMods;

	public bool preventPermanentWounds;

	public bool dontMindRawFood;

	public Color? hairColorOverride;

	public Color? skinColorBase;

	public Color? skinColorOverride;

	public float randomBrightnessFactor;

	public TagFilter hairTagFilter;

	public TagFilter beardTagFilter;

	public GeneticBodyType? bodyType;

	public List<HeadTypeDef> forcedHeadTypes;

	public float minMelanin = -1f;

	public HairDef forcedHair;

	public bool womenCanHaveBeards;

	public float socialFightChanceFactor = 1f;

	public float aggroMentalBreakSelectionChanceFactor = 1f;

	public float mentalBreakMtbDays;

	public MentalBreakDef mentalBreakDef;

	public float missingGeneRomanceChanceFactor = 1f;

	public float prisonBreakMTBFactor = 1f;

	public int biostatCpx = 1;

	public int biostatMet;

	public int biostatArc;

	public List<string> exclusionTags;

	public GeneDef prerequisite;

	public float selectionWeight = 1f;

	public bool canGenerateInGeneSet = true;

	public GeneSymbolPack symbolPack;

	public float marketValueFactor = 1f;

	public bool removeOnRedress;

	public bool passOnDirectly = true;

	public float selectionWeightFactorDarkSkin = 1f;

	public float selectionWeightCultist = 1f;

	[Unsaved(false)]
	private string cachedDescription;

	[Unsaved(false)]
	private Texture2D cachedIcon;

	public Texture2D Icon
	{
		get
		{
			if (cachedIcon == null)
			{
				if (iconPath.NullOrEmpty())
				{
					cachedIcon = BaseContent.BadTex;
				}
				else
				{
					cachedIcon = ContentFinder<Texture2D>.Get(iconPath) ?? BaseContent.BadTex;
				}
			}
			return cachedIcon;
		}
	}

	public Color IconColor
	{
		get
		{
			if (iconColor.HasValue)
			{
				return iconColor.Value;
			}
			if (skinColorBase.HasValue)
			{
				return skinColorBase.Value;
			}
			if (skinColorOverride.HasValue)
			{
				return skinColorOverride.Value;
			}
			if (hairColorOverride.HasValue)
			{
				return hairColorOverride.Value;
			}
			return Color.white;
		}
	}

	public string LabelShortAdj
	{
		get
		{
			if (!labelShortAdj.NullOrEmpty())
			{
				return labelShortAdj;
			}
			return label;
		}
	}

	public string DescriptionFull => cachedDescription ?? (cachedDescription = GetDescriptionFull());

	public bool HasDefinedGraphicProperties => !renderNodeProperties.NullOrEmpty();

	public List<PawnRenderNodeProperties> RenderNodeProperties => renderNodeProperties;

	public bool RandomChosen
	{
		get
		{
			if (randomChosen)
			{
				return true;
			}
			if (biostatArc > 0 || biostatCpx != 0 || biostatMet != 0)
			{
				return false;
			}
			if (!hairColorOverride.HasValue && !skinColorBase.HasValue && !skinColorOverride.HasValue && !bodyType.HasValue && forcedHeadTypes.NullOrEmpty() && hairTagFilter == null && beardTagFilter == null)
			{
				return HasDefinedGraphicProperties;
			}
			return true;
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (displayCategory == null)
		{
			displayCategory = GeneCategoryDefOf.Miscellaneous;
		}
		if (renderNodeProperties != null)
		{
			for (int i = 0; i < renderNodeProperties.Count; i++)
			{
				renderNodeProperties[i].ResolveReferencesRecursive();
			}
		}
	}

	public int AptitudeFor(SkillDef skill)
	{
		int num = 0;
		if (aptitudes.NullOrEmpty())
		{
			return num;
		}
		for (int i = 0; i < aptitudes.Count; i++)
		{
			if (aptitudes[i].skill == skill)
			{
				num += aptitudes[i].level;
			}
		}
		return num;
	}

	private string GetDescriptionFull()
	{
		StringBuilder sb = new StringBuilder();
		if (!description.NullOrEmpty())
		{
			sb.Append(description).AppendLine().AppendLine();
		}
		bool flag = false;
		if (prerequisite != null)
		{
			sb.AppendLine("Requires".Translate() + ": " + prerequisite.LabelCap);
			flag = true;
		}
		if (minAgeActive > 0f)
		{
			sb.AppendLine(string.Concat("TakesEffectAfterAge".Translate() + ": ", minAgeActive.ToString()));
			flag = true;
		}
		if (flag)
		{
			sb.AppendLine();
		}
		bool flag2 = false;
		if (biostatCpx != 0)
		{
			sb.AppendLineTagged("Complexity".Translate().Colorize(GeneUtility.GCXColor) + ": " + biostatCpx.ToStringWithSign());
			flag2 = true;
		}
		if (biostatMet != 0)
		{
			sb.AppendLineTagged("Metabolism".Translate().CapitalizeFirst().Colorize(GeneUtility.METColor) + ": " + biostatMet.ToStringWithSign());
			flag2 = true;
		}
		if (biostatArc != 0)
		{
			sb.AppendLineTagged("ArchitesRequired".Translate().Colorize(GeneUtility.ARCColor) + ": " + biostatArc.ToStringWithSign());
			flag2 = true;
		}
		if (flag2)
		{
			sb.AppendLine();
		}
		if (forcedTraits != null)
		{
			sb.AppendLineTagged(("ForcedTraits".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
			sb.Append(forcedTraits.Select((GeneticTraitData x) => x.def.DataAtDegree(x.degree).label).ToLineList("  - ", capitalizeItems: true)).AppendLine().AppendLine();
		}
		if (suppressedTraits != null)
		{
			sb.AppendLineTagged(("SuppressedTraits".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
			sb.Append(suppressedTraits.Select((GeneticTraitData x) => x.def.DataAtDegree(x.degree).label).ToLineList("  - ", capitalizeItems: true)).AppendLine().AppendLine();
		}
		if (aptitudes != null)
		{
			sb.AppendLineTagged(("Aptitudes".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
			sb.Append(aptitudes.Select((Aptitude x) => x.skill.LabelCap.ToString() + " " + x.level.ToStringWithSign()).ToLineList("  - ", capitalizeItems: true)).AppendLine().AppendLine();
		}
		bool effectsTitleWritten = false;
		if (passionMod != null)
		{
			switch (passionMod.modType)
			{
			case PassionMod.PassionModType.AddOneLevel:
				AppendEffectLine("PassionModAdd".Translate(passionMod.skill));
				break;
			case PassionMod.PassionModType.DropAll:
				AppendEffectLine("PassionModDrop".Translate(passionMod.skill));
				break;
			}
		}
		if (!statFactors.NullOrEmpty())
		{
			for (int num = 0; num < statFactors.Count; num++)
			{
				StatModifier statModifier = statFactors[num];
				if (statModifier.stat.CanShowWithLoadedMods())
				{
					AppendEffectLine(statModifier.stat.LabelCap + " " + statModifier.ToStringAsFactor);
				}
			}
		}
		if (!conditionalStatAffecters.NullOrEmpty())
		{
			for (int num2 = 0; num2 < conditionalStatAffecters.Count; num2++)
			{
				if (conditionalStatAffecters[num2].statFactors.NullOrEmpty())
				{
					continue;
				}
				for (int num3 = 0; num3 < conditionalStatAffecters[num2].statFactors.Count; num3++)
				{
					StatModifier statModifier2 = conditionalStatAffecters[num2].statFactors[num3];
					if (statModifier2.stat.CanShowWithLoadedMods())
					{
						AppendEffectLine(statModifier2.stat.LabelCap + " " + statModifier2.ToStringAsFactor + " (" + conditionalStatAffecters[num2].Label + ")");
					}
				}
			}
		}
		if (!statOffsets.NullOrEmpty())
		{
			for (int num4 = 0; num4 < statOffsets.Count; num4++)
			{
				StatModifier statModifier3 = statOffsets[num4];
				if (statModifier3.stat.CanShowWithLoadedMods())
				{
					AppendEffectLine(statModifier3.stat.LabelCap + " " + statModifier3.ValueToStringAsOffset);
				}
			}
		}
		if (!conditionalStatAffecters.NullOrEmpty())
		{
			for (int num5 = 0; num5 < conditionalStatAffecters.Count; num5++)
			{
				if (conditionalStatAffecters[num5].statOffsets.NullOrEmpty())
				{
					continue;
				}
				for (int num6 = 0; num6 < conditionalStatAffecters[num5].statOffsets.Count; num6++)
				{
					StatModifier statModifier4 = conditionalStatAffecters[num5].statOffsets[num6];
					if (statModifier4.stat.CanShowWithLoadedMods())
					{
						AppendEffectLine(statModifier4.stat.LabelCap + " " + statModifier4.ValueToStringAsOffset + " (" + conditionalStatAffecters[num5].Label.UncapitalizeFirst() + ")");
					}
				}
			}
		}
		if (!capMods.NullOrEmpty())
		{
			for (int num7 = 0; num7 < capMods.Count; num7++)
			{
				PawnCapacityModifier pawnCapacityModifier = capMods[num7];
				if (pawnCapacityModifier.offset != 0f)
				{
					AppendEffectLine(pawnCapacityModifier.capacity.GetLabelFor().CapitalizeFirst() + " " + (pawnCapacityModifier.offset * 100f).ToString("+#;-#") + "%");
				}
				if (pawnCapacityModifier.postFactor != 1f)
				{
					AppendEffectLine(pawnCapacityModifier.capacity.GetLabelFor().CapitalizeFirst() + " x" + pawnCapacityModifier.postFactor.ToStringPercent());
				}
				if (pawnCapacityModifier.setMax != 999f)
				{
					AppendEffectLine(pawnCapacityModifier.capacity.GetLabelFor().CapitalizeFirst() + " " + "max".Translate().CapitalizeFirst() + ": " + pawnCapacityModifier.setMax.ToStringPercent());
				}
			}
		}
		if (!customEffectDescriptions.NullOrEmpty())
		{
			foreach (string customEffectDescription in customEffectDescriptions)
			{
				AppendEffectLine(customEffectDescription.ResolveTags());
			}
		}
		if (!damageFactors.NullOrEmpty())
		{
			for (int num8 = 0; num8 < damageFactors.Count; num8++)
			{
				AppendEffectLine("DamageType".Translate(damageFactors[num8].damageDef.label).CapitalizeFirst() + " x" + damageFactors[num8].factor.ToStringPercent());
			}
		}
		if (resourceLossPerDay != 0f && !resourceLabel.NullOrEmpty())
		{
			AppendEffectLine("ResourceLossPerDay".Translate(resourceLabel.Named("RESOURCE"), (-Mathf.RoundToInt(resourceLossPerDay * 100f)).ToStringWithSign().Named("OFFSET")).CapitalizeFirst());
		}
		if (!Mathf.Approximately(painFactor, 1f))
		{
			AppendEffectLine("Pain".Translate() + " x" + painFactor.ToStringPercent());
		}
		if (painOffset != 0f)
		{
			AppendEffectLine("Pain".Translate() + " " + (painOffset * 100f).ToString("+###0;-###0") + "%");
		}
		if (chemical != null)
		{
			if (!Mathf.Approximately(addictionChanceFactor, 1f))
			{
				if (addictionChanceFactor <= 0f)
				{
					AppendEffectLine("AddictionImmune".Translate(chemical).CapitalizeFirst());
				}
				else
				{
					AppendEffectLine("AddictionChanceFactor".Translate(chemical).CapitalizeFirst() + " x" + addictionChanceFactor.ToStringPercent());
				}
			}
			if (overdoseChanceFactor != 1f)
			{
				AppendEffectLine("OverdoseChanceFactor".Translate(chemical).CapitalizeFirst() + " x" + overdoseChanceFactor.ToStringPercent());
			}
			if (toleranceBuildupFactor != 1f)
			{
				AppendEffectLine("ToleranceBuildupFactor".Translate(chemical).CapitalizeFirst() + " x" + toleranceBuildupFactor.ToStringPercent());
			}
		}
		if (!enablesNeeds.NullOrEmpty())
		{
			if (enablesNeeds.Count == 1)
			{
				AppendEffectLine(string.Format("{0}: {1}", "AddsNeed".Translate(), enablesNeeds[0].LabelCap));
			}
			else
			{
				AppendEffectLine(string.Format("{0}: {1}", "AddsNeeds".Translate(), enablesNeeds.Select((NeedDef x) => x.label).ToCommaList().CapitalizeFirst()));
			}
		}
		if (!disablesNeeds.NullOrEmpty())
		{
			if (disablesNeeds.Count == 1)
			{
				AppendEffectLine(string.Format("{0}: {1}", "DisablesNeed".Translate(), disablesNeeds[0].LabelCap));
			}
			else
			{
				AppendEffectLine(string.Format("{0}: {1}", "DisablesNeeds".Translate(), disablesNeeds.Select((NeedDef x) => x.label).ToCommaList().CapitalizeFirst()));
			}
		}
		if (missingGeneRomanceChanceFactor != 1f)
		{
			AppendEffectLine("MissingGeneRomanceChance".Translate(label.Named("GENE")) + " x" + missingGeneRomanceChanceFactor.ToStringPercent());
		}
		if (ignoreDarkness)
		{
			AppendEffectLine("UnaffectedByDarkness".Translate());
		}
		if (foodPoisoningChanceFactor != 1f)
		{
			if (foodPoisoningChanceFactor <= 0f)
			{
				AppendEffectLine("FoodPoisoningImmune".Translate());
			}
			else
			{
				AppendEffectLine("Stat_Hediff_FoodPoisoningChanceFactor_Name".Translate() + " x" + foodPoisoningChanceFactor.ToStringPercent());
			}
		}
		if (socialFightChanceFactor != 1f)
		{
			if (socialFightChanceFactor <= 0f)
			{
				AppendEffectLine("WillNeverSocialFight".Translate());
			}
			else
			{
				AppendEffectLine("SocialFightChanceFactor".Translate() + " x" + socialFightChanceFactor.ToStringPercent());
			}
		}
		if (aggroMentalBreakSelectionChanceFactor != 1f)
		{
			if (aggroMentalBreakSelectionChanceFactor >= 999f)
			{
				AppendEffectLine("AlwaysAggroMentalBreak".Translate());
			}
			else if (aggroMentalBreakSelectionChanceFactor <= 0f)
			{
				AppendEffectLine("NeverAggroMentalBreak".Translate());
			}
			else
			{
				AppendEffectLine("AggroMentalBreakSelectionChanceFactor".Translate() + " x" + aggroMentalBreakSelectionChanceFactor.ToStringPercent());
			}
		}
		if (prisonBreakMTBFactor != 1f)
		{
			if (prisonBreakMTBFactor < 0f)
			{
				AppendEffectLine("WillNeverPrisonBreak".Translate());
			}
			else
			{
				AppendEffectLine("PrisonBreakIntervalFactor".Translate() + " x" + prisonBreakMTBFactor.ToStringPercent());
			}
		}
		bool flag3 = effectsTitleWritten;
		if (!makeImmuneTo.NullOrEmpty())
		{
			if (flag3)
			{
				sb.AppendLine();
			}
			sb.AppendLineTagged(("ImmuneTo".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
			sb.AppendLine(makeImmuneTo.Select((HediffDef x) => x.label).ToLineList("  - ", capitalizeItems: true));
			flag3 = true;
		}
		if (!hediffGiversCannotGive.NullOrEmpty())
		{
			if (flag3)
			{
				sb.AppendLine();
			}
			sb.AppendLineTagged(("ImmuneTo".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
			sb.AppendLine(hediffGiversCannotGive.Select((HediffDef x) => x.label).ToLineList("  - ", capitalizeItems: true));
			flag3 = true;
		}
		if (biologicalAgeTickFactorFromAgeCurve != null)
		{
			if (flag3)
			{
				sb.AppendLine();
			}
			sb.AppendLineTagged(("AgeFactors".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
			sb.AppendLine(biologicalAgeTickFactorFromAgeCurve.Select((CurvePoint p) => "PeriodYears".Translate(p.x).ToString() + ": x" + p.y.ToStringPercent()).ToLineList("  - ", capitalizeItems: true));
			flag3 = true;
		}
		if (disabledWorkTags != WorkTags.None)
		{
			if (flag3)
			{
				sb.AppendLine();
			}
			IEnumerable<WorkTypeDef> source = DefDatabase<WorkTypeDef>.AllDefsListForReading.Where((WorkTypeDef x) => (disabledWorkTags & x.workTags) != 0);
			sb.AppendLineTagged(("DisabledWorkLabel".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
			sb.AppendLine("  - " + source.Select((WorkTypeDef x) => x.labelShort).ToCommaList().CapitalizeFirst());
			if (disabledWorkTags.ExactlyOneWorkTagSet())
			{
				sb.AppendLine("  - " + disabledWorkTags.LabelTranslated().CapitalizeFirst());
			}
			flag3 = true;
		}
		if (!abilities.NullOrEmpty())
		{
			if (flag3)
			{
				sb.AppendLine();
			}
			if (abilities.Count == 1)
			{
				sb.AppendLineTagged(("GivesAbility".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
			}
			else
			{
				sb.AppendLineTagged(("GivesAbilities".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
			}
			sb.AppendLine(abilities.Select((AbilityDef x) => x.label).ToLineList("  - ", capitalizeItems: true));
			flag3 = true;
		}
		IEnumerable<ThoughtDef> enumerable = DefDatabase<ThoughtDef>.AllDefs.Where((ThoughtDef x) => (x.requiredGenes.NotNullAndContains(this) || x.nullifyingGenes.NotNullAndContains(this)) && x.stages != null && x.stages.Any((ThoughtStage y) => y.baseMoodEffect != 0f));
		if (enumerable.Any())
		{
			if (flag3)
			{
				sb.AppendLine();
			}
			sb.AppendLineTagged(("Mood".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
			foreach (ThoughtDef item in enumerable)
			{
				ThoughtStage thoughtStage = item.stages.FirstOrDefault((ThoughtStage x) => x.baseMoodEffect != 0f);
				if (thoughtStage != null)
				{
					string text = thoughtStage.LabelCap + ": " + thoughtStage.baseMoodEffect.ToStringWithSign();
					if (item.requiredGenes.NotNullAndContains(this))
					{
						sb.AppendLine("  - " + text);
					}
					else if (item.nullifyingGenes.NotNullAndContains(this))
					{
						sb.AppendLine("  - " + "Removes".Translate() + ": " + text);
					}
				}
			}
		}
		return sb.ToString().TrimEndNewlines();
		void AppendEffectLine(string text2)
		{
			if (!effectsTitleWritten)
			{
				sb.AppendLineTagged(("Effects".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
				effectsTitleWritten = true;
			}
			sb.AppendLine("  - " + text2);
		}
	}

	public bool ConflictsWith(GeneDef other)
	{
		if (this == other)
		{
			return true;
		}
		if (exclusionTags != null && other.exclusionTags != null)
		{
			for (int i = 0; i < exclusionTags.Count; i++)
			{
				if (other.exclusionTags.Contains(exclusionTags[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		if (painFactor != 1f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Pain".Translate(), "x" + painFactor.ToStringPercent(), "Stat_Hediff_Pain_Desc".Translate(), 4050);
		}
		if (painOffset != 0f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Pain".Translate(), (painOffset * 100f).ToString("+###0;-###0") + "%", "Stat_Hediff_Pain_Desc".Translate(), 4050);
		}
		if (missingGeneRomanceChanceFactor != 1f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "MissingGeneRomanceChance".Translate(LabelCap.Named("GENE")), "x" + missingGeneRomanceChanceFactor.ToStringPercent(), "StatsReport_MissingGeneRomanceChance".Translate(), 4050);
		}
		if (forcedTraits != null)
		{
			string text = forcedTraits.Select((GeneticTraitData x) => x.def.DataAtDegree(x.degree).label).ToLineList(null, capitalizeItems: true);
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "ForcedTraits".Translate(), text, "ForcedTraitsDesc".Translate() + "\n\n" + text, 4080);
		}
		if (aptitudes != null)
		{
			string text2 = aptitudes.Select((Aptitude x) => x.skill.LabelCap.ToString() + " " + x.level.ToStringWithSign()).ToLineList(null, capitalizeItems: true);
			yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "Aptitudes".Translate().CapitalizeFirst(), text2, "AptitudesDesc".Translate() + "\n\n" + text2, 4090);
		}
		if (statOffsets != null)
		{
			for (int i = 0; i < statOffsets.Count; i++)
			{
				StatModifier statModifier = statOffsets[i];
				if (statModifier.stat.CanShowWithLoadedMods())
				{
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifier.stat.LabelCap, statModifier.ValueToStringAsOffset, statModifier.stat.description, 4070);
				}
			}
		}
		if (statFactors != null)
		{
			for (int i = 0; i < statFactors.Count; i++)
			{
				StatModifier statModifier2 = statFactors[i];
				if (statModifier2.stat.CanShowWithLoadedMods())
				{
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifier2.stat.LabelCap, statModifier2.ToStringAsFactor, statModifier2.stat.description, 4070);
				}
			}
		}
		if (capMods != null)
		{
			for (int i = 0; i < capMods.Count; i++)
			{
				PawnCapacityModifier capMod = capMods[i];
				if (capMod.offset != 0f)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, capMod.capacity.GetLabelFor().CapitalizeFirst(), (capMod.offset * 100f).ToString("+#;-#") + "%", capMod.capacity.description, 4060);
				}
				if (capMod.postFactor != 1f)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, capMod.capacity.GetLabelFor().CapitalizeFirst(), "x" + capMod.postFactor.ToStringPercent(), capMod.capacity.description, 4060);
				}
				if (capMod.SetMaxDefined && req.Pawn != null)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, capMod.capacity.GetLabelFor().CapitalizeFirst(), "max".Translate().CapitalizeFirst() + " " + capMod.EvaluateSetMax(req.Pawn).ToStringPercent(), capMod.capacity.description, 4060);
				}
			}
		}
		if (biostatCpx != 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "Complexity".Translate().CapitalizeFirst(), biostatCpx.ToString(), "ComplexityDesc".Translate(), 998);
		}
		if (biostatMet != 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "Metabolism".Translate().CapitalizeFirst(), biostatMet.ToString(), "MetabolismDesc".Translate(), 997);
		}
		if (biostatArc != 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Genetics, "ArchitesRequired".Translate().CapitalizeFirst(), biostatArc.ToString(), "ArchitesRequiredDesc".Translate(), 995);
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (geneClass == null)
		{
			yield return "geneClass is null";
		}
		if (!typeof(Gene).IsAssignableFrom(geneClass))
		{
			yield return "geneClass is not Gene or subclass thereof";
		}
		if (mentalBreakMtbDays > 0f && mentalBreakDef == null)
		{
			yield return "mentalBreakMtbDays is >0 with null mentalBreakDef";
		}
		if (!HasDefinedGraphicProperties)
		{
			yield break;
		}
		foreach (PawnRenderNodeProperties renderNodeProperty in renderNodeProperties)
		{
			foreach (string item in renderNodeProperty.ConfigErrors())
			{
				yield return item;
			}
		}
	}
}
