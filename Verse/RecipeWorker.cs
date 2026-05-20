using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class RecipeWorker
{
	public RecipeDef recipe;

	public virtual bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		return true;
	}

	public virtual AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
	{
		return AvailableOnNow(thing, part);
	}

	public virtual IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
	{
		return Enumerable.Empty<BodyPartRecord>();
	}

	public virtual void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
	}

	public virtual bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
	{
		if (pawn.Faction == billDoerFaction && !pawn.IsQuestLodger())
		{
			return false;
		}
		return recipe.isViolation;
	}

	public virtual string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
	{
		return recipe.label;
	}

	public virtual void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
	{
		ingredient.Destroy();
	}

	public virtual void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
	{
	}

	public virtual void CheckForWarnings(Pawn billDoer)
	{
	}

	public virtual float GetIngredientCount(IngredientCount ing, Bill bill)
	{
		return ing.GetBaseCount();
	}

	public virtual TaggedString GetConfirmation(Pawn pawn)
	{
		return null;
	}

	protected void ReportViolation(Pawn pawn, Pawn billDoer, Faction factionToInform, int goodwillImpact, HistoryEventDef overrideEventDef = null)
	{
		if (factionToInform != null && billDoer != null && billDoer.Faction == Faction.OfPlayer)
		{
			Faction.OfPlayer.TryAffectGoodwillWith(factionToInform, goodwillImpact, canSendMessage: true, !factionToInform.temporary, overrideEventDef ?? HistoryEventDefOf.PerformedHarmfulSurgery);
			QuestUtility.SendQuestTargetSignals(pawn.questTags, "SurgeryViolation", pawn.Named("SUBJECT"));
		}
	}

	public virtual string LabelFromUniqueIngredients(Bill bill)
	{
		return null;
	}
}
