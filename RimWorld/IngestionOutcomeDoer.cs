using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public abstract class IngestionOutcomeDoer
{
	public float chance = 1f;

	public bool doToGeneratedPawnIfAddicted;

	public void DoIngestionOutcome(Pawn pawn, Thing ingested, int ingestedCount)
	{
		if (Rand.Value < chance)
		{
			DoIngestionOutcomeSpecial(pawn, ingested, ingestedCount);
		}
	}

	protected abstract void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount);

	public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
	{
		return Enumerable.Empty<StatDrawEntry>();
	}
}
