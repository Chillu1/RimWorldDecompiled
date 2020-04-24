using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Recipe_AdministerIngestible : Recipe_Surgery
	{
		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			float num = ingredients[0].Ingested(pawn, (pawn.needs != null && pawn.needs.food != null) ? pawn.needs.food.NutritionWanted : 0f);
			if (!pawn.Dead)
			{
				pawn.needs.food.CurLevel += num;
			}
			if (pawn.needs.mood != null)
			{
				if (pawn.IsTeetotaler() && ingredients[0].def.IsNonMedicalDrug)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ForcedMeToTakeDrugs, billDoer);
				}
				else if (pawn.IsProsthophobe() && ingredients[0].def == ThingDefOf.Luciferium)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ForcedMeToTakeLuciferium, billDoer);
				}
			}
		}

		public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
		{
		}

		public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
		{
			if (pawn.Faction == billDoerFaction)
			{
				return false;
			}
			ThingDef thingDef = recipe.ingredients[0].filter.AllowedThingDefs.First();
			if (thingDef.IsNonMedicalDrug)
			{
				foreach (CompProperties comp in thingDef.comps)
				{
					CompProperties_Drug compProperties_Drug = comp as CompProperties_Drug;
					if (compProperties_Drug != null && compProperties_Drug.chemical != null && compProperties_Drug.chemical.addictionHediff != null && pawn.health.hediffSet.HasHediff(compProperties_Drug.chemical.addictionHediff))
					{
						return false;
					}
				}
			}
			return thingDef.IsNonMedicalDrug;
		}

		public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.IsTeetotaler() && recipe.ingredients[0].filter.BestThingRequest.singleDef.IsNonMedicalDrug)
			{
				return base.GetLabelWhenUsedOn(pawn, part) + " (" + "TeetotalerUnhappy".Translate() + ")";
			}
			if (pawn.IsProsthophobe() && recipe.ingredients[0].filter.BestThingRequest.singleDef == ThingDefOf.Luciferium)
			{
				return base.GetLabelWhenUsedOn(pawn, part) + " (" + "ProsthophobeUnhappy".Translate() + ")";
			}
			return base.GetLabelWhenUsedOn(pawn, part);
		}
	}
}
