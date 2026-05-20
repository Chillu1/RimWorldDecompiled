using System;

namespace RimWorld;

public class BookOutcomeProperties_GainAnomalyResearch : BookOutcomeProperties_GainResearch
{
	public override Type DoerClass => typeof(ReadingOutcomeDoerGainAnomalyResearch);
}
