using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_RemoveBodyPart_Cut : Recipe_RemoveBodyPart
{
	protected override bool SpawnPartsWhenRemoved => false;

	public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
	{
		return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, (BodyPartRecord record) => !pawn.health.hediffSet.PartIsMissing(record));
	}

	public override void ApplyThoughts(Pawn pawn, Pawn billDoer)
	{
		if (pawn.Dead)
		{
			ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, billDoer, PawnExecutionKind.GenericBrutal);
		}
	}

	public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
	{
		return "Cut".Translate();
	}
}
