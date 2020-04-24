using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Recipe_InstallImplant : Recipe_Surgery
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate(BodyPartRecord record)
			{
				if (!pawn.health.hediffSet.GetNotMissingParts().Contains(record))
				{
					return false;
				}
				if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
				{
					return false;
				}
				return (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && (x.def == recipe.addsHediff || !recipe.CompatibleWithHediff(x.def)))) ? true : false;
			});
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			if (billDoer != null)
			{
				if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
			}
			pawn.health.AddHediff(recipe.addsHediff, part);
		}
	}
}
