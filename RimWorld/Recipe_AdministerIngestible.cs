using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Recipe_AdministerIngestible : Recipe_Surgery
{
	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		float num = ingredients[0].Ingested(pawn, (pawn.needs?.food?.NutritionWanted).GetValueOrDefault());
		if (!pawn.Dead)
		{
			if (pawn.needs?.food != null)
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
		if (billDoer != null)
		{
			if (ingredients[0].def.IsDrug)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AdministeredDrug, billDoer.Named(HistoryEventArgsNames.Doer)));
			}
			if (ingredients[0].def.IsDrug && ingredients[0].def.ingestible.drugCategory == DrugCategory.Hard)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AdministeredHardDrug, billDoer.Named(HistoryEventArgsNames.Doer)));
			}
			if (ingredients[0].def.IsNonMedicalDrug)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AdministeredRecreationalDrug, billDoer.Named(HistoryEventArgsNames.Doer)));
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
			if (AddictionUtility.HasChemicalDependency(pawn, thingDef))
			{
				return false;
			}
			foreach (CompProperties comp in thingDef.comps)
			{
				if (comp is CompProperties_Drug compProperties_Drug && compProperties_Drug.chemical?.addictionHediff != null && pawn.health.hediffSet.HasHediff(compProperties_Drug.chemical.addictionHediff))
				{
					return false;
				}
			}
		}
		return thingDef.IsNonMedicalDrug;
	}

	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		if (!base.AvailableOnNow(thing, part))
		{
			return false;
		}
		Pawn pawn = thing as Pawn;
		if (pawn.IsMutant)
		{
			if (!pawn.mutant.Def.canUseDrugs)
			{
				return false;
			}
			ThingDef item = recipe.ingredients[0].filter.AllowedThingDefs.First();
			if (!pawn.mutant.Def.drugWhitelist.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
	{
		ThingDef singleDef = recipe.ingredients[0].filter.BestThingRequest.singleDef;
		if (singleDef.IsNonMedicalDrug && pawn.PawnWouldBeUnhappyTakingDrug(singleDef))
		{
			return base.GetLabelWhenUsedOn(pawn, part) + " (" + "TeetotalerUnhappy".Translate() + ")";
		}
		if (pawn.IsProsthophobe() && singleDef == ThingDefOf.Luciferium)
		{
			return base.GetLabelWhenUsedOn(pawn, part) + " (" + "ProsthophobeUnhappy".Translate() + ")";
		}
		return base.GetLabelWhenUsedOn(pawn, part);
	}
}
