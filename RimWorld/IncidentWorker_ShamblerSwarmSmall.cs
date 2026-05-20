using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IncidentWorker_ShamblerSwarmSmall : IncidentWorker_ShamblerSwarm
{
	private static readonly IntRange NumShamblersToSpawn = new IntRange(2, 4);

	protected override IntRange ShamblerLifespanTicksRange => new IntRange(25000, 45000);

	protected override List<Pawn> GenerateEntities(IncidentParms parms, float points)
	{
		int randomInRange = NumShamblersToSpawn.RandomInRange;
		List<Pawn> list = new List<Pawn>();
		for (int i = 0; i < randomInRange; i++)
		{
			Pawn item = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.ShamblerSwarmer, Faction.OfEntities));
			list.Add(item);
		}
		SetupShamblerHediffs(list, ShamblerLifespanTicksRange);
		return list;
	}

	protected override void SendLetter(IncidentParms parms, List<Pawn> entities)
	{
		string letterLabel = def.letterLabel;
		TaggedString baseLetterText = def.letterText.Formatted(entities.Count);
		SendStandardLetter(letterLabel, baseLetterText, def.letterDef, parms, entities);
	}
}
