using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_ExecuteByCut : RecipeWorker
{
	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
		{
			ReportViolation(pawn, billDoer, pawn.HomeFaction, -100);
		}
		ExecutionUtility.DoExecutionByCut(billDoer, pawn);
		ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, billDoer, PawnExecutionKind.GenericHumane);
	}
}
