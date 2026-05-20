using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ThoughtDef : Def
{
	public Type thoughtClass;

	public Type workerClass;

	public List<ThoughtStage> stages = new List<ThoughtStage>();

	public int stackLimit = 1;

	public float stackedEffectMultiplier = 0.75f;

	public bool stagesStack;

	public float durationDays;

	public bool invert;

	public bool validWhileDespawned;

	public ThoughtDef nextThought;

	public ThoughtDef producesMemoryThought;

	public List<TraitDef> nullifyingTraits;

	public List<TraitDef> neverNullifyIfAnyTrait;

	[XmlInheritanceAllowDuplicateNodes]
	public List<TraitRequirement> nullifyingTraitDegrees;

	public List<TaleDef> nullifyingOwnTales;

	public List<PreceptDef> nullifyingPrecepts;

	public List<HediffDef> nullifyingHediffs;

	public List<GeneDef> nullifyingGenes;

	public List<TraitDef> requiredTraits;

	public List<GeneDef> requiredGenes;

	public int requiredTraitsDegree = int.MinValue;

	public List<HediffDef> requiredHediffs;

	public StatDef effectMultiplyingStat;

	public SimpleCurve effectMultiplyingStatCurve;

	public HediffDef hediff;

	public MentalStateDef mentalState;

	public GameConditionDef gameCondition;

	public bool nullifiedIfNotColonist;

	public ThoughtDef thoughtToMake;

	[NoTranslate]
	private string icon;

	public bool showBubble;

	public ExpectationDef minExpectation;

	public ExpectationDef minExpectationForNegativeThought;

	public List<ThoughtDef> replaceThoughts;

	public DevelopmentalStage developmentalStageFilter = DevelopmentalStage.Child | DevelopmentalStage.Adult;

	public ChemicalDef chemicalDef;

	public bool lerpMoodToZero;

	public int stackLimitForSameOtherPawn = -1;

	public float lerpOpinionToZeroAfterDurationPct = 0.7f;

	public DevelopmentalStage socialTargetDevelopmentalStageFilter = DevelopmentalStage.Child | DevelopmentalStage.Adult;

	public bool ignoreSubhumans = true;

	public bool doNotApplyToQuestLodgers;

	public float maxCumulatedOpinionOffset = float.MaxValue;

	public TaleDef taleDef;

	public Gender gender;

	[MustTranslate]
	public string labelInBracketsExtraForHediff;

	[Unsaved(false)]
	private ThoughtWorker workerInt;

	[Unsaved(false)]
	private BoolUnknown isMemoryCached = BoolUnknown.Unknown;

	private Texture2D iconInt;

	public string Label
	{
		get
		{
			if (!label.NullOrEmpty())
			{
				return label;
			}
			if (!stages.NullOrEmpty())
			{
				if (!stages[0].label.NullOrEmpty())
				{
					return stages[0].label;
				}
				if (!stages[0].labelSocial.NullOrEmpty())
				{
					return stages[0].labelSocial;
				}
			}
			Log.Error("Cannot get good label for ThoughtDef " + defName);
			return defName;
		}
	}

	public override TaggedString LabelCap
	{
		get
		{
			if (Label.NullOrEmpty())
			{
				return null;
			}
			if (cachedLabelCap.NullOrEmpty())
			{
				cachedLabelCap = Label.CapitalizeFirst();
			}
			return cachedLabelCap;
		}
	}

	public int DurationTicks => (int)(durationDays * 60000f);

	public bool IsMemory
	{
		get
		{
			if (isMemoryCached == BoolUnknown.Unknown)
			{
				isMemoryCached = ((!(durationDays > 0f) && !typeof(Thought_Memory).IsAssignableFrom(thoughtClass)) ? BoolUnknown.False : BoolUnknown.True);
			}
			return isMemoryCached == BoolUnknown.True;
		}
	}

	public bool IsSituational => Worker != null;

	public bool IsSocial => typeof(ISocialThought).IsAssignableFrom(ThoughtClass);

	public bool RequiresSpecificTraitsDegree => requiredTraitsDegree != int.MinValue;

	public ThoughtWorker Worker
	{
		get
		{
			if (workerInt == null && workerClass != null)
			{
				workerInt = (ThoughtWorker)Activator.CreateInstance(workerClass);
				workerInt.def = this;
			}
			return workerInt;
		}
	}

	public Type ThoughtClass
	{
		get
		{
			if (thoughtClass != null)
			{
				return thoughtClass;
			}
			if (IsMemory)
			{
				return typeof(Thought_Memory);
			}
			return typeof(Thought_Situational);
		}
	}

	public Texture2D Icon
	{
		get
		{
			if (iconInt == null)
			{
				if (icon == null)
				{
					return null;
				}
				iconInt = ContentFinder<Texture2D>.Get(icon);
			}
			return iconInt;
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (stages.NullOrEmpty())
		{
			yield return "no stages";
		}
		if (workerClass != null && nextThought != null)
		{
			yield return "has a nextThought but also has a workerClass. nextThought only works for memories";
		}
		if (IsMemory && workerClass != null)
		{
			yield return "has a workerClass but is a memory. workerClass only works for situational thoughts, not memories";
		}
		if (!IsMemory && workerClass == null && IsSituational)
		{
			yield return "is a situational thought but has no workerClass. Situational thoughts require workerClasses to analyze the situation";
		}
		for (int i = 0; i < stages.Count; i++)
		{
			if (stages[i] == null)
			{
				continue;
			}
			foreach (string item2 in stages[i].ConfigErrors())
			{
				yield return item2;
			}
		}
	}

	public static ThoughtDef Named(string defName)
	{
		return DefDatabase<ThoughtDef>.GetNamed(defName);
	}
}
