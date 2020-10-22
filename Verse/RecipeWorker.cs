using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class RecipeWorker
	{
		public RecipeDef recipe;

		public virtual bool AvailableOnNow(Thing thing)
		{
			return true;
		}

		public virtual IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			yield break;
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

		protected void ReportViolation(Pawn pawn, Pawn billDoer, Faction factionToInform, int goodwillImpact, string reason)
		{
			if (factionToInform != null && billDoer != null && billDoer.Faction != null)
			{
				factionToInform.TryAffectGoodwillWith(billDoer.Faction, goodwillImpact, canSendMessage: true, reason: reason, lookTarget: pawn, canSendHostilityLetter: !factionToInform.temporary);
				QuestUtility.SendQuestTargetSignals(pawn.questTags, "SurgeryViolation", pawn.Named("SUBJECT"));
			}
		}
	}
}
