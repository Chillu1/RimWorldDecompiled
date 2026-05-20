using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_ChangeImplantLevel : Recipe_Surgery
{
	private bool Operable(Hediff target, RecipeDef recipe)
	{
		int hediffLevelOffset = recipe.hediffLevelOffset;
		if (hediffLevelOffset == 0)
		{
			return false;
		}
		if (!(target is Hediff_Level { level: var level } hediff_Level))
		{
			return false;
		}
		if (hediff_Level.def == recipe.changesHediffLevel)
		{
			if (hediffLevelOffset <= 0)
			{
				return level > 0;
			}
			return (float)level < hediff_Level.def.maxSeverity;
		}
		return false;
	}

	public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
	{
		return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, (BodyPartRecord record) => pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == record && Operable(x, recipe)) ? true : false);
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (billDoer != null)
		{
			TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
		}
		Hediff_Level hediff_Level = (Hediff_Level)pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => Operable(h, recipe) && h.Part == part);
		if (hediff_Level != null)
		{
			if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
			{
				ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
			}
			hediff_Level.ChangeLevel(recipe.hediffLevelOffset);
		}
	}
}
