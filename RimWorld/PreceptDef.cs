using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PreceptDef : Def
{
	public Type preceptClass = typeof(Precept);

	public IssueDef issue;

	public List<PreceptComp> comps = new List<PreceptComp>();

	public List<AbilityStatModifiers> abilityStatFactors;

	public List<StatModifier> statOffsets;

	public List<StatModifier> statFactors;

	public List<ConditionalStatAffecter> conditionalStatAffecters;

	public float selectionWeight = 1f;

	public List<WorkTypeDef> opposedWorkTypes = new List<WorkTypeDef>();

	public PreceptImpact impact;

	public List<MemeDef> associatedMemes = new List<MemeDef>();

	public List<MemeDef> conflictingMemes = new List<MemeDef>();

	public List<MemeDef> requiredMemes = new List<MemeDef>();

	public bool visible = true;

	public bool visibleOnAddFloatMenu = true;

	public bool listedForRoles = true;

	public PreceptDef takeNameFrom;

	public PreceptDef alsoAdds;

	public int maxCount = 1;

	public bool isNonIdeoRitual;

	[NoTranslate]
	public List<string> exclusionTags = new List<string>();

	public bool allowDuplicates;

	public bool ignoreLimitsInEditMode;

	public bool canUseAlreadyUsedThingDef;

	public bool classic;

	public bool classicExtra;

	public float defaultSelectionWeight;

	public bool enabledForNPCFactions = true;

	public bool countsTowardsPreceptLimit = true;

	public bool canGenerateAsSpecialPrecept = true;

	public RulePackDef nameMaker;

	public int nameMaxLength = 16;

	public SimpleCurve preceptInstanceCountCurve;

	public RitualPatternDef ritualPatternBase;

	public bool receivesExpectationsQualityOffset;

	public bool usesIdeoVisualEffects = true;

	public bool mergeRitualGizmosFromAllIdeos;

	public bool allowSpectatorsFromOtherIdeos = true;

	public bool allowOptionalRitualObligations;

	public bool classicModeOnly;

	public List<PreceptThingChanceClass> buildingDefChances;

	public List<ExpectationDef> buildingMinExpectations;

	public List<RoomRequirement> buildingRoomRequirements;

	public List<RoomRequirement> buildingRoomRequirementsFixed;

	public SimpleCurve roomRequirementCountCurve;

	public bool leaderRole;

	public int activationBelieverCount = -1;

	public int deactivationBelieverCount = -1;

	public List<RoleRequirement> roleRequirements;

	public WorkTags roleDisabledWorkTags;

	public WorkTags roleRequiredWorkTags;

	public WorkTags roleRequiredWorkTagAny;

	public List<PreceptApparelRequirement> roleApparelRequirements;

	public SimpleCurve roleApparelRequirementCountCurve;

	public List<AbilityDef> grantedAbilities;

	public List<RoleEffect> roleEffects;

	public List<string> roleTags;

	public string iconPath;

	public float restrictToSupremeGenderChance;

	public float certaintyLossFactor = 1f;

	public float convertPowerFactor = 1f;

	public int expectationsOffset;

	public bool createsRoleEmptyThought = true;

	public bool roleCanBeChild;

	public bool disallowLoggingCamps;

	public bool disallowMiningCamps;

	public bool disallowHuntingCamps;

	public bool disallowFarmingCamps;

	public bool approvesOfSlavery;

	public bool prefersNudity;

	public Gender genderPrefersNudity;

	public bool useChoicesFromBuildingDefs;

	public int displayOrderInImpact;

	public int displayOrderInIssue;

	public bool proselytizes;

	public int requiredScars;

	public bool approvesOfCharity;

	public float blindPawnChance = -1f;

	public bool approvesOfBlindness;

	public bool canRemoveInUI = true;

	public bool prefersDarkness;

	public List<NeedDef> enablesNeeds;

	public List<NeedDef> disablesNeeds;

	public float blindPsychicSensitivityOffset;

	public DrugPolicyDef defaultDrugPolicyOverride;

	public bool warnPlayerOnDesignateChopTree;

	public bool warnPlayerOnDesignateMine;

	public bool willingToConstructOtherIdeoBuildables;

	public float biosculpterPodCycleSpeedFactor = 1f;

	public float growthVatSpeedFactor = 1f;

	public bool prefersSlabBed;

	public bool useRepeatPenalty = true;

	public bool showRitualFloatMenuOption = true;

	[MustTranslate]
	public string tipLabelOverride;

	public bool capitalizeAsTitle = true;

	public bool ignoreNameUniqueness;

	[MustTranslate]
	public string extraTextPawnDeathLetter;

	public PreceptDef apparelPreceptSwapDef;

	public AbilityGroupDef useCooldownFromAbilityGroupDef;

	public bool iconIgnoresIdeoColor;

	public PreceptDef sourcePawnRoleDef;

	public AbilityDef sourceAbilityDef;

	public bool likesHumanLeatherApparel;

	public bool approvesOfRaiding;

	public float skipOpportunityLettersBeforeDay = 10f;

	public bool notifyPlayerOnOpportunity = true;

	public Type workerClass = typeof(PreceptWorker);

	private PreceptWorker worker;

	[Unsaved(false)]
	private List<TraitRequirement> traitsAffectingCached;

	[Unsaved(false)]
	private List<string> requiredMemeLabels;

	private Texture2D icon;

	public Texture2D Icon
	{
		get
		{
			if (icon == null)
			{
				if (iconPath != null)
				{
					icon = ContentFinder<Texture2D>.Get(iconPath);
				}
				else if (!issue.iconPath.NullOrEmpty())
				{
					icon = issue.Icon;
				}
			}
			return icon;
		}
	}

	public PreceptWorker Worker
	{
		get
		{
			if (worker == null)
			{
				worker = (PreceptWorker)Activator.CreateInstance(workerClass);
				worker.def = this;
			}
			return worker;
		}
	}

	public List<TraitRequirement> TraitsAffecting
	{
		get
		{
			if (traitsAffectingCached == null)
			{
				traitsAffectingCached = new List<TraitRequirement>();
				for (int i = 0; i < comps.Count; i++)
				{
					traitsAffectingCached.AddRange(comps[i].TraitsAffecting);
				}
				traitsAffectingCached.RemoveDuplicates((TraitRequirement a, TraitRequirement b) => a.def == b.def && (a.degree.HasValue ? a.degree.Value : 0) == (b.degree.HasValue ? b.degree.Value : 0));
			}
			return traitsAffectingCached;
		}
	}

	public List<string> RequiredMemeLabels
	{
		get
		{
			if (requiredMemeLabels == null && !requiredMemes.NullOrEmpty())
			{
				requiredMemeLabels = new List<string>();
				foreach (MemeDef requiredMeme in requiredMemes)
				{
					requiredMemeLabels.Add(requiredMeme.label);
				}
			}
			return requiredMemeLabels;
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (preceptClass == typeof(Precept_Ritual) && ritualPatternBase == null)
		{
			yield return "Ritual doesn't define a ritualPatternBase, was it meant to be abstract?";
		}
		if (typeof(Precept_ThingStyle).IsAssignableFrom(preceptClass))
		{
			foreach (PreceptThingChance thingDef in Worker.ThingDefs)
			{
				ThingDef def = thingDef.def;
				if (!def.CanBeStyled())
				{
					yield return "ThingDef " + def.defName + " is on available things list of " + defName + " precept, but missing CompStyleable thing comp!";
				}
			}
		}
		foreach (PreceptComp comp in comps)
		{
			foreach (string item2 in comp.ConfigErrors(this))
			{
				yield return item2;
			}
		}
	}

	public override void PostLoad()
	{
		base.PostLoad();
		foreach (PreceptComp comp in comps)
		{
			comp.preceptDef = this;
		}
	}
}
