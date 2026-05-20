using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IngestionOutcomeDoer_UseThing : IngestionOutcomeDoer
{
	protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
	{
		ingested.TryGetComp<CompUsable>().UsedBy(pawn);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
	{
		yield break;
	}
}
