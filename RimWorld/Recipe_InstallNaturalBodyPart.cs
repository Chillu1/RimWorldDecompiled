using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Recipe_InstallNaturalBodyPart : Recipe_Surgery
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate(BodyPartRecord record)
			{
				if (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record))
				{
					return false;
				}
				if (record.parent != null && !pawn.health.hediffSet.GetNotMissingParts().Contains(record.parent))
				{
					return false;
				}
				return (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) || pawn.health.hediffSet.HasDirectlyAddedPartFor(record)) ? true : false;
			});
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			if (billDoer != null && !CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
			{
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
				MedicalRecipesUtility.RestorePartAndSpawnAllPreviousParts(pawn, part, billDoer.Position, billDoer.Map);
			}
		}
	}
}
