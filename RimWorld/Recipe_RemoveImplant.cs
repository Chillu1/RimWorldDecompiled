using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Recipe_RemoveImplant : Recipe_Surgery
{
	public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
	{
		List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < allHediffs.Count; i++)
		{
			if (allHediffs[i].Part != null && allHediffs[i].def == recipe.removesHediff && allHediffs[i].Visible)
			{
				yield return allHediffs[i].Part;
			}
		}
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		MedicalRecipesUtility.IsClean(pawn, part);
		bool flag = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
		if (billDoer != null)
		{
			if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
			{
				return;
			}
			TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
			if (!pawn.health.hediffSet.GetNotMissingParts().Contains(part))
			{
				return;
			}
			Hediff hediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) => x.def == recipe.removesHediff);
			if (hediff != null)
			{
				if (hediff.def.spawnThingOnRemoved != null)
				{
					GenSpawn.Spawn(hediff.def.spawnThingOnRemoved, billDoer.Position, billDoer.Map);
				}
				pawn.health.RemoveHediff(hediff);
			}
		}
		if (flag)
		{
			ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
		}
	}
}
