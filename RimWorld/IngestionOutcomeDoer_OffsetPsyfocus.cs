using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IngestionOutcomeDoer_OffsetPsyfocus : IngestionOutcomeDoer
{
	public float offset;

	protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
	{
		pawn.psychicEntropy?.OffsetPsyfocusDirectly(offset * (float)ingestedCount);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
	{
		if (ModsConfig.RoyaltyActive)
		{
			string text = ((offset >= 0f) ? "+" : string.Empty);
			yield return new StatDrawEntry(StatCategoryDefOf.Drug, "Psyfocus".Translate(), text + offset.ToStringPercent(), "PsyfocusDesc".Translate() + ".", 1000);
		}
	}
}
