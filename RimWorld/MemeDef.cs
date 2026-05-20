using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class MemeDef : Def
{
	public string iconPath;

	public int renderOrder;

	public int impact = -999;

	public MemeCategory category;

	public MemeGroupDef groupDef;

	[NoTranslate]
	public List<string> exclusionTags = new List<string>();

	public List<List<PreceptDef>> requireOne;

	public PreceptsWithNoneChance selectOneOrNone;

	public List<RequiredRitualAndBuilding> requiredRituals;

	public IdeoWeaponClassPair preferredWeaponClasses;

	public float randomizationSelectionWeightFactor = 1f;

	public List<ThingDef> requireAnyRitualSeat;

	public IntRange deityCount = new IntRange(-1, -1);

	public bool allowDuringTutorial;

	public List<FactionDef> factionWhitelist;

	public bool hiddenInChooseMemes;

	public RulePack generalRules;

	public IdeoDescriptionMaker descriptionMaker;

	public RulePackDef deityNameMakerOverride;

	public RulePackDef deityTypeMakerOverride;

	public bool allowSymbolsFromDeity = true;

	public bool symbolPackOverride;

	[MustTranslate]
	public List<IdeoSymbolPack> symbolPacks;

	public List<DeityNameType> fixedDeityNameTypes;

	public List<ThingStyleCategoryWithPriority> thingStyleCategories;

	public IntRange ritualsToMake = IntRange.Zero;

	public List<string> replaceRitualsWithTags = new List<string>();

	public List<RitualPatternDef> replacementPatterns;

	public List<ThingDef> consumableBuildings;

	public int veneratedAnimalsCountOffset;

	public int veneratedAnimalsCountOverride = -1;

	public List<StyleItemTagWeighted> styleItemTags;

	[MustTranslate]
	public string worshipRoomLabel;

	public List<ResearchProjectDef> startingResearchProjects = new List<ResearchProjectDef>();

	public List<BuildableDef> addDesignators;

	public List<DesignatorDropdownGroupDef> addDesignatorGroups;

	public List<PreceptApparelRequirement> apparelRequirements;

	[XmlInheritanceAllowDuplicateNodes]
	public List<TraitRequirement> agreeableTraits;

	[XmlInheritanceAllowDuplicateNodes]
	public List<TraitRequirement> disagreeableTraits;

	public bool preventApparelRequirements;

	public XenotypeSet xenotypeSet;

	private Texture2D icon;

	private List<BuildableDef> cachedAllDesignatorBuildables;

	private List<string> unlockedRoles;

	private List<string> unlockedRituals;

	private Ideo unlockedRolesCachedFor;

	public Texture2D Icon
	{
		get
		{
			if (!ModLister.CheckIdeology("Memes"))
			{
				return BaseContent.BadTex;
			}
			if (icon == null)
			{
				icon = ContentFinder<Texture2D>.Get(iconPath);
			}
			return icon;
		}
	}

	public List<BuildableDef> AllDesignatorBuildables
	{
		get
		{
			if (cachedAllDesignatorBuildables == null)
			{
				cachedAllDesignatorBuildables = new List<BuildableDef>();
				if (addDesignators != null)
				{
					foreach (BuildableDef addDesignator in addDesignators)
					{
						cachedAllDesignatorBuildables.Add(addDesignator);
					}
				}
				if (addDesignatorGroups != null)
				{
					foreach (DesignatorDropdownGroupDef addDesignatorGroup in addDesignatorGroups)
					{
						cachedAllDesignatorBuildables.AddRange(addDesignatorGroup.BuildablesWithoutDefaultDesignators());
					}
				}
			}
			return cachedAllDesignatorBuildables;
		}
	}

	public List<string> UnlockedRoles(Ideo ideo)
	{
		if (unlockedRoles == null || ideo != unlockedRolesCachedFor)
		{
			unlockedRoles = new List<string>();
			foreach (PreceptDef item in DefDatabase<PreceptDef>.AllDefsListForReading)
			{
				if (typeof(Precept_Role).IsAssignableFrom(item.preceptClass) && !item.requiredMemes.NullOrEmpty() && item.requiredMemes.Contains(this))
				{
					unlockedRoles.Add(item.LabelCap.Resolve());
				}
			}
			unlockedRolesCachedFor = ideo;
		}
		return unlockedRoles;
	}

	public List<string> UnlockedRituals()
	{
		if (unlockedRituals == null)
		{
			unlockedRituals = new List<string>();
			if (consumableBuildings != null)
			{
				foreach (RitualPatternDef p in DefDatabase<RitualPatternDef>.AllDefsListForReading)
				{
					if (p.ignoreConsumableBuildingRequirement || p.ritualObligationTargetFilter == null || p.ritualObligationTargetFilter.thingDefs.NullOrEmpty() || !p.ritualObligationTargetFilter.thingDefs.Any((ThingDef td) => consumableBuildings.Contains(td)))
					{
						continue;
					}
					object obj = p.shortDescOverride.CapitalizeFirst();
					if (obj == null)
					{
						TaggedString? taggedString = DefDatabase<PreceptDef>.AllDefsListForReading.FirstOrDefault((PreceptDef pr) => pr.ritualPatternBase == p)?.LabelCap;
						obj = (taggedString.HasValue ? ((string)taggedString.GetValueOrDefault()) : null);
					}
					string text = (string)obj;
					if (text != null)
					{
						unlockedRituals.Add(text);
					}
				}
			}
		}
		return unlockedRituals;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (category == MemeCategory.Structure && string.IsNullOrWhiteSpace(worshipRoomLabel))
		{
			yield return "Structure meme has empty worshipRoomLabel";
		}
		if (fixedDeityNameTypes != null && category != MemeCategory.Structure)
		{
			yield return "fixedDeityNameTypes can only be used in structure memes.";
		}
		if (!thingStyleCategories.NullOrEmpty())
		{
			foreach (ThingStyleCategoryWithPriority thingStyleCategory in thingStyleCategories)
			{
				if (thingStyleCategory.priority <= 0f)
				{
					yield return "style category " + thingStyleCategory.category.LabelCap + " has <= 0 priority. It must be positive.";
				}
			}
		}
		if (category == MemeCategory.Structure && impact != 0)
		{
			yield return defName + " structure meme impact must be 0.";
		}
		if (category != MemeCategory.Structure && (impact < 1 || impact > 3))
		{
			yield return $"{defName} normal meme impact must be between 1 and {3}.";
		}
		if (!agreeableTraits.NullOrEmpty() && !disagreeableTraits.NullOrEmpty() && agreeableTraits.SharesElementWith(disagreeableTraits))
		{
			yield return defName + " agreeableTraits and disagreeableTraits share one or more values.";
		}
		if (category != MemeCategory.Structure)
		{
			yield break;
		}
		if (descriptionMaker == null)
		{
			yield return "descriptionMaker is required for structure memes.";
			yield break;
		}
		if (descriptionMaker.patterns.NullOrEmpty())
		{
			yield return "descriptionMaker must define at least one pattern for structure memes.";
		}
		foreach (IdeoDescriptionMaker.PatternEntry pattern in descriptionMaker.patterns)
		{
			if (pattern.def == null)
			{
				yield return "descriptionMaker pattern has null def.";
			}
		}
	}
}
