using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class FoodRestrictionDatabase : IExposable
	{
		private List<FoodRestriction> foodRestrictions = new List<FoodRestriction>();

		public List<FoodRestriction> AllFoodRestrictions => foodRestrictions;

		public FoodRestrictionDatabase()
		{
			GenerateStartingFoodRestrictions();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref foodRestrictions, "foodRestrictions", LookMode.Deep);
			BackCompatibility.PostExposeData(this);
		}

		public FoodRestriction DefaultFoodRestriction()
		{
			if (foodRestrictions.Count == 0)
			{
				MakeNewFoodRestriction();
			}
			return foodRestrictions[0];
		}

		public AcceptanceReport TryDelete(FoodRestriction foodRestriction)
		{
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
			{
				if (item.foodRestriction != null && item.foodRestriction.CurrentFoodRestriction == foodRestriction)
				{
					return new AcceptanceReport("FoodRestrictionInUse".Translate(item));
				}
			}
			foreach (Pawn item2 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
			{
				if (item2.foodRestriction != null && item2.foodRestriction.CurrentFoodRestriction == foodRestriction)
				{
					item2.foodRestriction.CurrentFoodRestriction = null;
				}
			}
			foodRestrictions.Remove(foodRestriction);
			return AcceptanceReport.WasAccepted;
		}

		public FoodRestriction MakeNewFoodRestriction()
		{
			int id = (!foodRestrictions.Any()) ? 1 : (foodRestrictions.Max((FoodRestriction o) => o.id) + 1);
			FoodRestriction foodRestriction = new FoodRestriction(id, "FoodRestriction".Translate() + " " + id.ToString());
			foodRestriction.filter.SetAllow(ThingCategoryDefOf.Foods, allow: true);
			foodRestriction.filter.SetAllow(ThingCategoryDefOf.CorpsesHumanlike, allow: true);
			foodRestriction.filter.SetAllow(ThingCategoryDefOf.CorpsesAnimal, allow: true);
			foodRestrictions.Add(foodRestriction);
			return foodRestriction;
		}

		private void GenerateStartingFoodRestrictions()
		{
			MakeNewFoodRestriction().label = "FoodRestrictionLavish".Translate();
			FoodRestriction foodRestriction = MakeNewFoodRestriction();
			foodRestriction.label = "FoodRestrictionFine".Translate();
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.ingestible != null && (int)allDef.ingestible.preferability >= 9 && allDef != ThingDefOf.InsectJelly)
				{
					foodRestriction.filter.SetAllow(allDef, allow: false);
				}
			}
			FoodRestriction foodRestriction2 = MakeNewFoodRestriction();
			foodRestriction2.label = "FoodRestrictionSimple".Translate();
			foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef2.ingestible != null && (int)allDef2.ingestible.preferability >= 8 && allDef2 != ThingDefOf.InsectJelly)
				{
					foodRestriction2.filter.SetAllow(allDef2, allow: false);
				}
			}
			foodRestriction2.filter.SetAllow(ThingDefOf.MealSurvivalPack, allow: false);
			FoodRestriction foodRestriction3 = MakeNewFoodRestriction();
			foodRestriction3.label = "FoodRestrictionPaste".Translate();
			foreach (ThingDef allDef3 in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef3.ingestible != null && (int)allDef3.ingestible.preferability >= 7 && allDef3 != ThingDefOf.MealNutrientPaste && allDef3 != ThingDefOf.InsectJelly && allDef3 != ThingDefOf.Pemmican)
				{
					foodRestriction3.filter.SetAllow(allDef3, allow: false);
				}
			}
			FoodRestriction foodRestriction4 = MakeNewFoodRestriction();
			foodRestriction4.label = "FoodRestrictionRaw".Translate();
			foreach (ThingDef allDef4 in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef4.ingestible != null && (int)allDef4.ingestible.preferability >= 6)
				{
					foodRestriction4.filter.SetAllow(allDef4, allow: false);
				}
			}
			foodRestriction4.filter.SetAllow(ThingDefOf.Chocolate, allow: false);
			FoodRestriction foodRestriction5 = MakeNewFoodRestriction();
			foodRestriction5.label = "FoodRestrictionNothing".Translate();
			foodRestriction5.filter.SetDisallowAll();
		}
	}
}
