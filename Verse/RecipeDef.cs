using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public class RecipeDef : Def
{
	public Type workerClass = typeof(RecipeWorker);

	public Type workerCounterClass = typeof(RecipeWorkerCounter);

	[MustTranslate]
	public string jobString = "Doing an unknown recipe.";

	public WorkTypeDef requiredGiverWorkType;

	public float workAmount = -1f;

	public StatDef workSpeedStat;

	public StatDef efficiencyStat;

	public StatDef workTableEfficiencyStat;

	public StatDef workTableSpeedStat;

	public List<IngredientCount> ingredients = new List<IngredientCount>();

	public ThingFilter fixedIngredientFilter = new ThingFilter();

	public ThingFilter defaultIngredientFilter;

	public bool allowMixingIngredients;

	public bool ignoreIngredientCountTakeEntireStacks;

	private Type ingredientValueGetterClass = typeof(IngredientValueGetter_Volume);

	public List<SpecialThingFilterDef> forceHiddenSpecialFilters;

	public bool autoStripCorpses = true;

	public bool interruptIfIngredientIsRotting;

	public List<ThingDefCountClass> products = new List<ThingDefCountClass>();

	public List<SpecialProductType> specialProducts;

	public bool productHasIngredientStuff;

	public bool useIngredientsForColor = true;

	public int targetCountAdjustment = 1;

	public ThingDef unfinishedThingDef;

	public List<SkillRequirement> skillRequirements;

	public SkillDef workSkill;

	public float workSkillLearnFactor = 1f;

	public EffecterDef effectWorking;

	public SoundDef soundWorking;

	private ThingDef uiIconThing;

	[NoTranslate]
	private string uiIconPath;

	public List<ThingDef> recipeUsers;

	public List<BodyPartDef> appliedOnFixedBodyParts = new List<BodyPartDef>();

	public List<BodyPartGroupDef> appliedOnFixedBodyPartGroups = new List<BodyPartGroupDef>();

	public HediffDef addsHediff;

	public HediffDef removesHediff;

	public HediffDef addsHediffOnFailure;

	public HediffDef changesHediffLevel;

	public List<string> incompatibleWithHediffTags;

	public int hediffLevelOffset;

	public bool hideBodyPartNames;

	public bool isViolation;

	[MustTranslate]
	public string successfullyRemovedHediffMessage;

	public float surgerySuccessChanceFactor = 1f;

	public float deathOnFailedSurgeryChance;

	public bool targetsBodyPart = true;

	public bool anesthetize = true;

	public int minPartCount = -1;

	public bool surgeryIgnoreEnvironment;

	public SurgeryOutcomeEffectDef surgeryOutcomeEffect;

	public Gender? genderPrerequisite;

	public bool mustBeFertile;

	public bool allowedForQuestLodgers = true;

	public int minAllowedAge = -1;

	public DevelopmentalStage? developmentalStageFilter;

	public bool humanlikeOnly;

	public List<MutantDef> mutantPrerequisite;

	public List<MutantDef> mutantBlacklist;

	public float smeltingWorkAmount = -1f;

	public ResearchProjectDef researchPrerequisite;

	public List<MemeDef> memePrerequisitesAny;

	public List<ResearchProjectDef> researchPrerequisites;

	[NoTranslate]
	public List<string> factionPrerequisiteTags;

	public bool fromIdeoBuildingPreceptOnly;

	public ConceptDef conceptLearned;

	public bool dontShowIfAnyIngredientMissing;

	public int displayPriority = 99999;

	public int formingTicks;

	public bool mechanitorOnlyRecipe;

	public int gestationCycles;

	public bool mechResurrection;

	[Unsaved(false)]
	private RecipeWorker workerInt;

	[Unsaved(false)]
	private RecipeWorkerCounter workerCounterInt;

	[Unsaved(false)]
	private IngredientValueGetter ingredientValueGetterInt;

	[Unsaved(false)]
	private List<ThingDef> premultipliedSmallIngredients;

	[Unsaved(false)]
	public bool regenerateOnDifficultyChange;

	[Unsaved(false)]
	public int adjustedCount = 1;

	[Unsaved(false)]
	private Texture2D cachedIcon;

	[Unsaved(false)]
	private bool resolvedCachedIcon;

	[Unsaved(false)]
	private bool? isSurgeryCached;

	public ThingDef UIIconThing => uiIconThing ?? ProducedThingDef;

	public Texture2D UIIcon
	{
		get
		{
			if (!resolvedCachedIcon)
			{
				resolvedCachedIcon = true;
				if (!uiIconPath.NullOrEmpty())
				{
					cachedIcon = ContentFinder<Texture2D>.Get(uiIconPath);
				}
			}
			return cachedIcon;
		}
	}

	public RecipeWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (RecipeWorker)Activator.CreateInstance(workerClass);
				workerInt.recipe = this;
			}
			return workerInt;
		}
	}

	public RecipeWorkerCounter WorkerCounter
	{
		get
		{
			if (workerCounterInt == null)
			{
				workerCounterInt = (RecipeWorkerCounter)Activator.CreateInstance(workerCounterClass);
				workerCounterInt.recipe = this;
			}
			return workerCounterInt;
		}
	}

	public IngredientValueGetter IngredientValueGetter
	{
		get
		{
			if (ingredientValueGetterInt == null)
			{
				ingredientValueGetterInt = (IngredientValueGetter)Activator.CreateInstance(ingredientValueGetterClass);
			}
			return ingredientValueGetterInt;
		}
	}

	public bool AvailableNow
	{
		get
		{
			if (researchPrerequisite != null && !researchPrerequisite.IsFinished)
			{
				return false;
			}
			if (memePrerequisitesAny != null)
			{
				bool flag = false;
				foreach (MemeDef item in memePrerequisitesAny)
				{
					if (Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(item))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (researchPrerequisites != null && researchPrerequisites.Any((ResearchProjectDef r) => !r.IsFinished))
			{
				return false;
			}
			bool unlockedByIdeo;
			if (factionPrerequisiteTags != null && factionPrerequisiteTags.Any((string tag) => Faction.OfPlayer.def.recipePrerequisiteTags == null || !Faction.OfPlayer.def.recipePrerequisiteTags.Contains(tag)))
			{
				unlockedByIdeo = false;
				Check();
				if (!unlockedByIdeo)
				{
					return false;
				}
			}
			if (fromIdeoBuildingPreceptOnly && (!ModsConfig.IdeologyActive || !IdeoUtility.PlayerHasPreceptForBuilding(ProducedThingDef)))
			{
				return false;
			}
			return true;
			void Check()
			{
				foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
				{
					foreach (Precept_Role item2 in allIdeo.RolesListForReading)
					{
						if (item2.apparelRequirements != null)
						{
							foreach (PreceptApparelRequirement apparelRequirement in item2.apparelRequirements)
							{
								ThingDef thingDef = apparelRequirement.requirement.AllRequiredApparel().FirstOrDefault();
								if (thingDef == null)
								{
									Log.Error("Apparel requirement for role " + item2.Label + " is null");
								}
								foreach (ThingDefCountClass product in products)
								{
									if (product.thingDef == thingDef)
									{
										unlockedByIdeo = true;
										return;
									}
								}
							}
						}
					}
				}
			}
		}
	}

	public string MinSkillString
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			if (skillRequirements != null)
			{
				for (int i = 0; i < skillRequirements.Count; i++)
				{
					SkillRequirement skillRequirement = skillRequirements[i];
					stringBuilder.AppendLine("   " + skillRequirement.skill.skillLabel.CapitalizeFirst() + ": " + skillRequirement.minLevel);
					flag = true;
				}
			}
			if (!flag)
			{
				stringBuilder.AppendLine("   (" + "NoneLower".Translate() + ")");
			}
			return stringBuilder.ToString();
		}
	}

	public IEnumerable<ThingDef> AllRecipeUsers
	{
		get
		{
			if (recipeUsers != null)
			{
				for (int i = 0; i < recipeUsers.Count; i++)
				{
					yield return recipeUsers[i];
				}
			}
			List<ThingDef> thingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < thingDefs.Count; i++)
			{
				if (thingDefs[i].recipes != null && thingDefs[i].recipes.Contains(this))
				{
					yield return thingDefs[i];
				}
			}
		}
	}

	public bool UsesUnfinishedThing => unfinishedThingDef != null;

	public bool IsSurgery
	{
		get
		{
			if (!isSurgeryCached.HasValue)
			{
				isSurgeryCached = false;
				foreach (ThingDef allRecipeUser in AllRecipeUsers)
				{
					if (allRecipeUser.category == ThingCategory.Pawn)
					{
						isSurgeryCached = true;
						break;
					}
				}
			}
			return isSurgeryCached.Value;
		}
	}

	public ThingDef ProducedThingDef
	{
		get
		{
			if (specialProducts != null)
			{
				return null;
			}
			if (products == null || products.Count != 1)
			{
				return null;
			}
			return products[0].thingDef;
		}
	}

	public bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		return Worker.AvailableOnNow(thing, part);
	}

	public float WorkAmountTotal(Thing thing)
	{
		if (thing == null)
		{
			return WorkAmountForStuff(null);
		}
		if (this == RecipeDefOf.SmeltOrDestroyThing && thing.Smeltable)
		{
			return smeltingWorkAmount;
		}
		return WorkAmountForStuff(thing.Stuff);
	}

	public float WorkAmountForStuff(ThingDef stuff)
	{
		if (workAmount >= 0f)
		{
			return workAmount;
		}
		return products[0].thingDef.GetStatValueAbstract(StatDefOf.WorkToMake, stuff);
	}

	public IEnumerable<ThingDef> PotentiallyMissingIngredients(Pawn billDoer, Map map)
	{
		for (int i = 0; i < ingredients.Count; i++)
		{
			IngredientCount ingredientCount = ingredients[i];
			bool flag = false;
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
			for (int j = 0; j < list.Count; j++)
			{
				Thing thing = list[j];
				if ((billDoer == null || !thing.IsForbidden(billDoer)) && !thing.Position.Fogged(map) && (ingredientCount.IsFixedIngredient || fixedIngredientFilter.Allows(thing)) && ingredientCount.filter.Allows(thing))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			if (ingredientCount.IsFixedIngredient)
			{
				yield return ingredientCount.filter.AllowedThingDefs.First();
				continue;
			}
			ThingDef thingDef = ingredientCount.filter.AllowedThingDefs.OrderBy((ThingDef x) => x.BaseMarketValue).FirstOrDefault((ThingDef x) => fixedIngredientFilter.Allows(x));
			if (thingDef != null)
			{
				yield return thingDef;
			}
		}
	}

	public bool IsIngredient(ThingDef th)
	{
		for (int i = 0; i < ingredients.Count; i++)
		{
			if (ingredients[i].filter.Allows(th) && (ingredients[i].IsFixedIngredient || fixedIngredientFilter.Allows(th)))
			{
				return true;
			}
		}
		return false;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (workerClass == null)
		{
			yield return "workerClass is null.";
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		DeepProfiler.Start("Stat refs");
		try
		{
			if (workTableSpeedStat == null)
			{
				workTableSpeedStat = StatDefOf.WorkTableWorkSpeedFactor;
			}
			if (workTableEfficiencyStat == null)
			{
				workTableEfficiencyStat = StatDefOf.WorkTableEfficiencyFactor;
			}
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("ingredients reference resolve");
		try
		{
			for (int i = 0; i < ingredients.Count; i++)
			{
				ingredients[i].ResolveReferences();
			}
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("fixedIngredientFilter.ResolveReferences()");
		try
		{
			fixedIngredientFilter?.ResolveReferences();
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("defaultIngredientFilter setup");
		try
		{
			if (defaultIngredientFilter == null)
			{
				defaultIngredientFilter = new ThingFilter();
				if (fixedIngredientFilter != null)
				{
					defaultIngredientFilter.CopyAllowancesFrom(fixedIngredientFilter);
				}
			}
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("defaultIngredientFilter.ResolveReferences()");
		try
		{
			defaultIngredientFilter.ResolveReferences();
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public bool CompatibleWithHediff(HediffDef hediffDef)
	{
		if (incompatibleWithHediffTags.NullOrEmpty() || hediffDef.tags.NullOrEmpty())
		{
			return true;
		}
		for (int i = 0; i < incompatibleWithHediffTags.Count; i++)
		{
			for (int j = 0; j < hediffDef.tags.Count; j++)
			{
				if (incompatibleWithHediffTags[i].Equals(hediffDef.tags[j], StringComparison.InvariantCultureIgnoreCase))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool PawnSatisfiesSkillRequirements(Pawn pawn)
	{
		return FirstSkillRequirementPawnDoesntSatisfy(pawn) == null;
	}

	public SkillRequirement FirstSkillRequirementPawnDoesntSatisfy(Pawn pawn)
	{
		if (skillRequirements == null)
		{
			return null;
		}
		for (int i = 0; i < skillRequirements.Count; i++)
		{
			if (!skillRequirements[i].PawnSatisfies(pawn))
			{
				return skillRequirements[i];
			}
		}
		return null;
	}

	public List<ThingDef> GetPremultipliedSmallIngredients()
	{
		if (premultipliedSmallIngredients != null)
		{
			return premultipliedSmallIngredients;
		}
		premultipliedSmallIngredients = (from td in ingredients.SelectMany((IngredientCount ingredient) => ingredient.filter.AllowedThingDefs)
			where td.smallVolume
			select td).Distinct().ToList();
		bool flag = true;
		while (flag)
		{
			flag = false;
			for (int num = 0; num < ingredients.Count; num++)
			{
				if (!ingredients[num].filter.AllowedThingDefs.Any((ThingDef td) => !premultipliedSmallIngredients.Contains(td)))
				{
					continue;
				}
				foreach (ThingDef allowedThingDef in ingredients[num].filter.AllowedThingDefs)
				{
					flag |= premultipliedSmallIngredients.Remove(allowedThingDef);
				}
			}
		}
		return premultipliedSmallIngredients;
	}

	private IEnumerable<Dialog_InfoCard.Hyperlink> GetIngredientsHyperlinks()
	{
		return Dialog_InfoCard.DefsToHyperlinks(from i in ingredients
			where i.IsFixedIngredient
			select i.FixedIngredient into i
			where i != null
			select i);
	}

	private IEnumerable<Dialog_InfoCard.Hyperlink> GetProductsHyperlinks()
	{
		return Dialog_InfoCard.DefsToHyperlinks(products.Select((ThingDefCountClass i) => i.thingDef));
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		if (workSkill != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Skill".Translate(), workSkill.LabelCap, "Stat_Recipe_Skill_Desc".Translate(), 4404);
		}
		if (!ingredients.NullOrEmpty())
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Ingredients".Translate(), ingredients.Select((IngredientCount ic) => ic.SummaryFor(this)).ToCommaList(), "Stat_Recipe_Ingredients_Desc".Translate(), 4405, null, GetIngredientsHyperlinks());
		}
		if (!skillRequirements.NullOrEmpty())
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "SkillRequirements".Translate(), skillRequirements.Select((SkillRequirement sr) => sr.Summary).ToCommaList(), "Stat_Recipe_SkillRequirements_Desc".Translate(), 4403);
		}
		if (products != null && products.Count > 0)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Products".Translate(), products.Select((ThingDefCountClass pr) => pr.Summary).ToCommaList(), "Stat_Recipe_Products_Desc".Translate(), 4405, null, GetProductsHyperlinks());
			float num = WorkAmountTotal(null);
			if (num > 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, StatDefOf.WorkToMake.LabelCap, num.ToStringWorkAmount(), StatDefOf.WorkToMake.description, StatDefOf.WorkToMake.displayPriorityInCategory);
			}
		}
		if (workSpeedStat != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "WorkSpeedStat".Translate(), workSpeedStat.LabelCap, "Stat_Recipe_WorkSpeedStat_Desc".Translate(), 4402);
		}
		if (efficiencyStat != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "EfficiencyStat".Translate(), efficiencyStat.LabelCap, "Stat_Recipe_EfficiencyStat_Desc".Translate(), 4401);
		}
		if (IsSurgery)
		{
			if (surgerySuccessChanceFactor >= 99999f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Surgery, "SurgerySuccessChanceFactor".Translate(), "Stat_Thing_Surgery_SuccessChanceFactor_CantFail".Translate(), "Stat_Thing_Surgery_SuccessChanceFactor_CantFail_Desc".Translate(), 4102);
			}
			else
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Surgery, "SurgerySuccessChanceFactor".Translate(), surgerySuccessChanceFactor.ToStringPercent(), "Stat_Thing_Surgery_SuccessChanceFactor_Desc".Translate(), 4102);
				if (deathOnFailedSurgeryChance >= 99999f)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Surgery, "SurgeryDeathOnFailChance".Translate(), "100%", "Stat_Thing_Surgery_DeathOnFailChance_Desc".Translate(), 4101);
				}
				else
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Surgery, "SurgeryDeathOnFailChance".Translate(), deathOnFailedSurgeryChance.ToStringPercent(), "Stat_Thing_Surgery_DeathOnFailChance_Desc".Translate(), 4101);
				}
			}
		}
		if (addsHediff == null)
		{
			yield break;
		}
		foreach (StatDrawEntry medicalStatsFromRecipeDef in MedicalRecipesUtility.GetMedicalStatsFromRecipeDefs(Gen.YieldSingle(this)))
		{
			yield return medicalStatsFromRecipeDef;
		}
	}
}
