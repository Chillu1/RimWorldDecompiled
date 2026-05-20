using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class FoodRestrictionDatabase : IExposable
{
	private List<FoodPolicy> foodRestrictions = new List<FoodPolicy>();

	public List<FoodPolicy> AllFoodRestrictions => foodRestrictions;

	public FoodRestrictionDatabase()
	{
		GenerateStartingFoodRestrictions();
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref foodRestrictions, "foodRestrictions", LookMode.Deep);
		BackCompatibility.PostExposeData(this);
	}

	public FoodPolicy DefaultFoodRestriction()
	{
		if (foodRestrictions.Count == 0)
		{
			MakeNewFoodRestriction();
		}
		return foodRestrictions[0];
	}

	public void SetDefault(FoodPolicy policy)
	{
		int index = foodRestrictions.IndexOf(policy);
		FoodPolicy value = foodRestrictions[0];
		foodRestrictions[0] = policy;
		foodRestrictions[index] = value;
	}

	public AcceptanceReport TryDelete(FoodPolicy foodPolicy)
	{
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
		{
			if (item.foodRestriction != null && item.foodRestriction.CurrentFoodPolicy == foodPolicy)
			{
				return new AcceptanceReport("FoodRestrictionInUse".Translate(item));
			}
		}
		foreach (Pawn item2 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
		{
			if (item2.foodRestriction != null && item2.foodRestriction.CurrentFoodPolicy == foodPolicy)
			{
				item2.foodRestriction.CurrentFoodPolicy = null;
			}
		}
		foodRestrictions.Remove(foodPolicy);
		return AcceptanceReport.WasAccepted;
	}

	public FoodPolicy MakeNewFoodRestriction()
	{
		int id = ((!foodRestrictions.Any()) ? 1 : (foodRestrictions.Max((FoodPolicy o) => o.id) + 1));
		FoodPolicy foodPolicy = new FoodPolicy(id, "FoodPolicy".Translate() + " " + id.ToString());
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.GetStatValueAbstract(StatDefOf.Nutrition) > 0f))
		{
			foodPolicy.filter.SetAllow(item, allow: true);
		}
		foodRestrictions.Add(foodPolicy);
		if (ModsConfig.IdeologyActive)
		{
			foodPolicy.filter.SetAllow(SpecialThingFilterDefOf.AllowVegetarian, allow: true);
			foodPolicy.filter.SetAllow(SpecialThingFilterDefOf.AllowCarnivore, allow: true);
			foodPolicy.filter.SetAllow(SpecialThingFilterDefOf.AllowCannibal, allow: true);
			foodPolicy.filter.SetAllow(SpecialThingFilterDefOf.AllowInsectMeat, allow: true);
		}
		if (ModsConfig.BiotechActive)
		{
			foodPolicy.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
		}
		return foodPolicy;
	}

	private void GenerateStartingFoodRestrictions()
	{
		MakeNewFoodRestriction().label = "FoodRestrictionLavish".Translate();
		FoodPolicy foodPolicy = MakeNewFoodRestriction();
		foodPolicy.label = "FoodRestrictionFine".Translate();
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef.ingestible != null && (int)allDef.ingestible.preferability >= 10 && allDef != ThingDefOf.InsectJelly)
			{
				foodPolicy.filter.SetAllow(allDef, allow: false);
			}
		}
		if (ModsConfig.BiotechActive)
		{
			foodPolicy.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
		}
		FoodPolicy foodPolicy2 = MakeNewFoodRestriction();
		foodPolicy2.label = "FoodRestrictionSimple".Translate();
		foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef2.ingestible != null && (int)allDef2.ingestible.preferability >= 9 && allDef2 != ThingDefOf.InsectJelly)
			{
				foodPolicy2.filter.SetAllow(allDef2, allow: false);
			}
		}
		foodPolicy2.filter.SetAllow(ThingDefOf.MealSurvivalPack, allow: false);
		if (ModsConfig.BiotechActive)
		{
			foodPolicy2.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
		}
		FoodPolicy foodPolicy3 = MakeNewFoodRestriction();
		foodPolicy3.label = "FoodRestrictionPaste".Translate();
		foreach (ThingDef allDef3 in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef3.ingestible != null && (int)allDef3.ingestible.preferability >= 8 && allDef3 != ThingDefOf.MealNutrientPaste && allDef3 != ThingDefOf.InsectJelly)
			{
				foodPolicy3.filter.SetAllow(allDef3, allow: false);
			}
		}
		if (ModsConfig.BiotechActive)
		{
			foodPolicy3.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
		}
		FoodPolicy foodPolicy4 = MakeNewFoodRestriction();
		foodPolicy4.label = "FoodRestrictionRaw".Translate();
		foreach (ThingDef allDef4 in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef4.ingestible != null && (int)allDef4.ingestible.preferability >= 7)
			{
				foodPolicy4.filter.SetAllow(allDef4, allow: false);
			}
		}
		foodPolicy4.filter.SetAllow(ThingDefOf.Chocolate, allow: false);
		if (ModsConfig.BiotechActive)
		{
			foodPolicy4.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
		}
		FoodPolicy foodPolicy5 = MakeNewFoodRestriction();
		foodPolicy5.label = "FoodRestrictionNothing".Translate();
		foodPolicy5.filter.SetDisallowAll();
		CreateIdeologyFoodRestrictions();
	}

	public void CreateIdeologyFoodRestrictions()
	{
		if (!ModsConfig.IdeologyActive)
		{
			return;
		}
		TaggedString vegLabel = "FoodRestrictionVegetarian".Translate();
		if (foodRestrictions.FirstOrDefault((FoodPolicy fr) => fr.label == vegLabel) == null)
		{
			FoodPolicy foodPolicy = MakeNewFoodRestriction();
			foodPolicy.label = vegLabel;
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (FoodUtility.UnacceptableVegetarian(allDef))
				{
					foodPolicy.filter.SetAllow(allDef, allow: false);
				}
			}
			foodPolicy.filter.SetAllow(SpecialThingFilterDefOf.AllowCarnivore, allow: false);
			foodPolicy.filter.SetAllow(SpecialThingFilterDefOf.AllowCannibal, allow: false);
			foodPolicy.filter.SetAllow(SpecialThingFilterDefOf.AllowInsectMeat, allow: false);
			if (ModsConfig.BiotechActive)
			{
				foodPolicy.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
			}
		}
		TaggedString carnivoreLabel = "FoodRestrictionCarnivore".Translate();
		if (foodRestrictions.FirstOrDefault((FoodPolicy fr) => fr.label == carnivoreLabel) == null)
		{
			FoodPolicy foodPolicy2 = MakeNewFoodRestriction();
			foodPolicy2.label = carnivoreLabel;
			foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
			{
				if (!FoodUtility.UnacceptableCarnivore(allDef2) && FoodUtility.GetMeatSourceCategory(allDef2) != MeatSourceCategory.Humanlike)
				{
					if (!allDef2.IsCorpse)
					{
						continue;
					}
					ThingDef sourceDef = allDef2.ingestible.sourceDef;
					if (sourceDef == null || sourceDef.race?.Humanlike != true)
					{
						continue;
					}
				}
				foodPolicy2.filter.SetAllow(allDef2, allow: false);
			}
			foodPolicy2.filter.SetAllow(SpecialThingFilterDefOf.AllowVegetarian, allow: false);
			foodPolicy2.filter.SetAllow(SpecialThingFilterDefOf.AllowCannibal, allow: false);
			foodPolicy2.filter.SetAllow(SpecialThingFilterDefOf.AllowInsectMeat, allow: false);
			if (ModsConfig.BiotechActive)
			{
				foodPolicy2.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
			}
		}
		TaggedString cannibalLabel = "FoodRestrictionCannibal".Translate();
		if (foodRestrictions.FirstOrDefault((FoodPolicy fr) => fr.label == cannibalLabel) == null)
		{
			FoodPolicy foodPolicy3 = MakeNewFoodRestriction();
			foodPolicy3.label = cannibalLabel;
			foreach (ThingDef allDef3 in DefDatabase<ThingDef>.AllDefs)
			{
				if (!FoodUtility.MaybeAcceptableCannibalDef(allDef3))
				{
					foodPolicy3.filter.SetAllow(allDef3, allow: false);
				}
			}
			foodPolicy3.filter.SetAllow(SpecialThingFilterDefOf.AllowVegetarian, allow: false);
			foodPolicy3.filter.SetAllow(SpecialThingFilterDefOf.AllowCarnivore, allow: false);
			foodPolicy3.filter.SetAllow(SpecialThingFilterDefOf.AllowInsectMeat, allow: false);
			if (ModsConfig.BiotechActive)
			{
				foodPolicy3.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
			}
		}
		TaggedString insectMeatLabel = "FoodRestrictionInsectMeat".Translate();
		if (foodRestrictions.FirstOrDefault((FoodPolicy fr) => fr.label == insectMeatLabel) != null)
		{
			return;
		}
		FoodPolicy foodPolicy4 = MakeNewFoodRestriction();
		foodPolicy4.label = insectMeatLabel;
		foreach (ThingDef allDef4 in DefDatabase<ThingDef>.AllDefs)
		{
			if (!FoodUtility.MaybeAcceptableInsectMeatEatersDef(allDef4))
			{
				foodPolicy4.filter.SetAllow(allDef4, allow: false);
			}
		}
		foodPolicy4.filter.SetAllow(SpecialThingFilterDefOf.AllowVegetarian, allow: false);
		foodPolicy4.filter.SetAllow(SpecialThingFilterDefOf.AllowCarnivore, allow: false);
		foodPolicy4.filter.SetAllow(SpecialThingFilterDefOf.AllowCannibal, allow: false);
		if (ModsConfig.BiotechActive)
		{
			foodPolicy4.filter.SetAllow(ThingDefOf.HemogenPack, allow: false);
		}
	}
}
