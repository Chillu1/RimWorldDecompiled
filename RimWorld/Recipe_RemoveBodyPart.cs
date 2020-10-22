using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	internal class Recipe_RemoveBodyPart : Recipe_Surgery
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			IEnumerable<BodyPartRecord> notMissingParts = pawn.health.hediffSet.GetNotMissingParts();
			foreach (BodyPartRecord part in notMissingParts)
			{
				if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
				{
					yield return part;
				}
				else if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part))
				{
					yield return part;
				}
				else if (part != pawn.RaceProps.body.corePart && part.def.canSuggestAmputation && pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury) && d.def.isBad && d.Visible && d.Part == part))
				{
					yield return part;
				}
			}
		}

		public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
		{
			if ((pawn.Faction == billDoerFaction || pawn.Faction == null) && !pawn.IsQuestLodger())
			{
				return false;
			}
			if (HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest)
			{
				return true;
			}
			return false;
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			bool flag = MedicalRecipesUtility.IsClean(pawn, part);
			bool flag2 = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
			if (billDoer != null)
			{
				if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
				MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, billDoer.Position, billDoer.Map);
				MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, billDoer.Position, billDoer.Map);
			}
			pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, null, part));
			if (flag)
			{
				if (pawn.Dead)
				{
					ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, PawnExecutionKind.OrganHarvesting);
				}
				ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn);
			}
			if (flag2)
			{
				ReportViolation(pawn, billDoer, pawn.FactionOrExtraMiniOrHomeFaction, -70, "GoodwillChangedReason_RemovedBodyPart".Translate(part.LabelShort));
			}
		}

		public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.RaceProps.IsMechanoid || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
			{
				return RecipeDefOf.RemoveBodyPart.label;
			}
			switch (HealthUtility.PartRemovalIntent(pawn, part))
			{
			case BodyPartRemovalIntent.Amputate:
				if (part.depth == BodyPartDepth.Inside || part.def.socketed)
				{
					return "RemoveOrgan".Translate();
				}
				return "Amputate".Translate();
			case BodyPartRemovalIntent.Harvest:
				return "HarvestOrgan".Translate();
			default:
				throw new InvalidOperationException();
			}
		}
	}
}
