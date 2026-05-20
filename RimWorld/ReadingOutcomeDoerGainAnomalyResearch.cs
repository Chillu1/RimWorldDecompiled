using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ReadingOutcomeDoerGainAnomalyResearch : ReadingOutcomeDoerGainResearch
{
	public new BookOutcomeProperties_GainAnomalyResearch Props => (BookOutcomeProperties_GainAnomalyResearch)props;

	public override int RoundTo { get; }

	protected override float GetBaseValue()
	{
		return BookUtility.GetAnomalyExpForQuality(base.Quality);
	}

	public override bool DoesProvidesOutcome(Pawn reader)
	{
		foreach (KeyValuePair<ResearchProjectDef, float> value2 in values)
		{
			value2.Deconstruct(out var key, out var _);
			if (key.CanStartNow)
			{
				return true;
			}
		}
		return false;
	}

	public override void OnReadingTick(Pawn reader, float factor)
	{
		foreach (KeyValuePair<ResearchProjectDef, float> value2 in values)
		{
			value2.Deconstruct(out var key, out var value);
			ResearchProjectDef researchProjectDef = key;
			float num = value;
			if (researchProjectDef.CanStartNow)
			{
				Find.ResearchManager.ApplyKnowledge(researchProjectDef, num * factor, out value);
			}
		}
	}
}
