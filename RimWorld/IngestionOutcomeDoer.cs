using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class IngestionOutcomeDoer
	{
		public float chance = 1f;

		public bool doToGeneratedPawnIfAddicted;

		public void DoIngestionOutcome(Pawn pawn, Thing ingested)
		{
			if (Rand.Value < chance)
			{
				DoIngestionOutcomeSpecial(pawn, ingested);
			}
		}

		protected abstract void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested);

		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			yield break;
		}
	}
}
