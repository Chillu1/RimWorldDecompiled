using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class TraitDegreeData : IRenderNodePropertiesParent
{
	[MustTranslate]
	public string label;

	[MustTranslate]
	public string labelMale;

	[MustTranslate]
	public string labelFemale;

	[Unsaved(false)]
	[TranslationHandle]
	public string untranslatedLabel;

	[MustTranslate]
	public string description;

	public int degree;

	public float commonality = 1f;

	public List<StatModifier> statOffsets;

	public List<StatModifier> statFactors;

	public ThinkTreeDef thinkTree;

	public MentalStateDef randomMentalState;

	public SimpleCurve randomMentalStateMtbDaysMoodCurve;

	public MentalStateDef forcedMentalState;

	public float forcedMentalStateMtbDays = -1f;

	public List<MentalStateDef> disallowedMentalStates;

	public List<ThoughtDef> disallowedThoughts;

	public List<TraitIngestionThoughtsOverride> disallowedThoughtsFromIngestion;

	public List<TraitIngestionThoughtsOverride> extraThoughtsFromIngestion;

	public List<InspirationDef> disallowedInspirations;

	public List<InspirationDef> mentalBreakInspirationGainSet;

	public string mentalBreakInspirationGainReasonText;

	public List<MeditationFocusDef> allowedMeditationFocusTypes;

	public List<MeditationFocusDef> disallowedMeditationFocusTypes;

	public float mentalBreakInspirationGainChance;

	public List<MentalBreakDef> theOnlyAllowedMentalBreaks;

	public List<SkillGain> skillGains = new List<SkillGain>();

	public float socialFightChanceFactor = 1f;

	public float marketValueFactorOffset;

	public float randomDiseaseMtbDays;

	public float hungerRateFactor = 1f;

	public float painOffset;

	public float painFactor = 1f;

	public Type mentalStateGiverClass = typeof(TraitMentalStateGiver);

	public List<AbilityDef> abilities;

	public List<IngestibleModifiers> ingestibleModifiers;

	public List<Aptitude> aptitudes;

	public List<NeedDef> enablesNeeds;

	public List<NeedDef> disablesNeeds;

	private List<PawnRenderNodeProperties> renderNodeProperties;

	public List<PossessionThingDefCountClass> possessions = new List<PossessionThingDefCountClass>();

	[Unsaved(false)]
	private TraitMentalStateGiver mentalStateGiverInt;

	[Unsaved(false)]
	private string cachedLabelCap;

	[Unsaved(false)]
	private string cachedLabelMaleCap;

	[Unsaved(false)]
	private string cachedLabelFemaleCap;

	[Unsaved(false)]
	private List<IssueDef> affectedIssuesCached;

	[Unsaved(false)]
	private List<MemeDef> agreeableMemesCached;

	[Unsaved(false)]
	private List<MemeDef> disagreeableMemesCached;

	public string LabelCap
	{
		get
		{
			if (cachedLabelCap == null)
			{
				cachedLabelCap = label.CapitalizeFirst();
			}
			return cachedLabelCap;
		}
	}

	public TraitMentalStateGiver MentalStateGiver
	{
		get
		{
			if (mentalStateGiverInt == null)
			{
				mentalStateGiverInt = (TraitMentalStateGiver)Activator.CreateInstance(mentalStateGiverClass);
				mentalStateGiverInt.traitDegreeData = this;
			}
			return mentalStateGiverInt;
		}
	}

	public bool HasDefinedGraphicProperties => !renderNodeProperties.NullOrEmpty();

	public List<PawnRenderNodeProperties> RenderNodeProperties => renderNodeProperties;

	public string GetLabelFor(Pawn pawn)
	{
		return GetLabelFor(pawn?.gender ?? Gender.None);
	}

	public string GetLabelCapFor(Pawn pawn)
	{
		return GetLabelCapFor(pawn?.gender ?? Gender.None);
	}

	public string GetLabelFor(Gender gender)
	{
		switch (gender)
		{
		case Gender.Male:
			if (!labelMale.NullOrEmpty())
			{
				return labelMale;
			}
			return label;
		case Gender.Female:
			if (!labelFemale.NullOrEmpty())
			{
				return labelFemale;
			}
			return label;
		default:
			return label;
		}
	}

	public string GetLabelCapFor(Gender gender)
	{
		switch (gender)
		{
		case Gender.Male:
			if (labelMale.NullOrEmpty())
			{
				return LabelCap;
			}
			if (cachedLabelMaleCap == null)
			{
				cachedLabelMaleCap = labelMale.CapitalizeFirst();
			}
			return cachedLabelMaleCap;
		case Gender.Female:
			if (labelFemale.NullOrEmpty())
			{
				return LabelCap;
			}
			if (cachedLabelFemaleCap == null)
			{
				cachedLabelFemaleCap = labelFemale.CapitalizeFirst();
			}
			return cachedLabelFemaleCap;
		default:
			return LabelCap;
		}
	}

	public List<IssueDef> GetAffectedIssues(TraitDef def)
	{
		if (affectedIssuesCached == null)
		{
			affectedIssuesCached = new List<IssueDef>();
			List<PreceptDef> allDefsListForReading = DefDatabase<PreceptDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (!affectedIssuesCached.Contains(allDefsListForReading[i].issue) && allDefsListForReading[i].TraitsAffecting.Any((TraitRequirement x) => x.def == def && x.degree.GetValueOrDefault() == degree))
				{
					affectedIssuesCached.Add(allDefsListForReading[i].issue);
				}
			}
		}
		return affectedIssuesCached;
	}

	public List<MemeDef> GetAffectedMemes(TraitDef def, bool agreeable)
	{
		if (agreeable)
		{
			if (agreeableMemesCached == null)
			{
				agreeableMemesCached = new List<MemeDef>();
				List<MemeDef> allDefsListForReading = DefDatabase<MemeDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (!allDefsListForReading[i].agreeableTraits.NullOrEmpty() && allDefsListForReading[i].agreeableTraits.Any((TraitRequirement x) => x.def == def && x.degree.GetValueOrDefault() == degree))
					{
						agreeableMemesCached.Add(allDefsListForReading[i]);
					}
				}
			}
			return agreeableMemesCached;
		}
		if (disagreeableMemesCached == null)
		{
			disagreeableMemesCached = new List<MemeDef>();
			List<MemeDef> allDefsListForReading2 = DefDatabase<MemeDef>.AllDefsListForReading;
			for (int num = 0; num < allDefsListForReading2.Count; num++)
			{
				if (!allDefsListForReading2[num].disagreeableTraits.NullOrEmpty() && allDefsListForReading2[num].disagreeableTraits.Any((TraitRequirement x) => x.def == def && x.degree.GetValueOrDefault() == degree))
				{
					disagreeableMemesCached.Add(allDefsListForReading2[num]);
				}
			}
		}
		return disagreeableMemesCached;
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

	public void PostLoad()
	{
		untranslatedLabel = label;
	}

	public void ResolveReferences()
	{
		if (renderNodeProperties != null)
		{
			for (int i = 0; i < renderNodeProperties.Count; i++)
			{
				renderNodeProperties[i].ResolveReferencesRecursive();
			}
		}
	}
}
