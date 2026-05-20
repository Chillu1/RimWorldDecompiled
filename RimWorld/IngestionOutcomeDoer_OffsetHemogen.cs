using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class IngestionOutcomeDoer_OffsetHemogen : IngestionOutcomeDoer
{
	public float offset;

	protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
	{
		GeneUtility.OffsetHemogen(pawn, offset * (float)ingestedCount);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
	{
		if (ModsConfig.BiotechActive)
		{
			string text = ((offset >= 0f) ? "+" : string.Empty);
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawnImportant, "Hemogen".Translate().CapitalizeFirst(), text + Mathf.RoundToInt(offset * 100f), "HemogenDesc".Translate(), 1000);
		}
	}
}
