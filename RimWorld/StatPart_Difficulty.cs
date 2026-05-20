using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

[Obsolete]
public class StatPart_Difficulty : StatPart
{
	private List<float> factorsPerDifficulty = new List<float>();

	public override void TransformValue(StatRequest req, ref float val)
	{
		val *= Multiplier(Find.Storyteller.difficultyDef);
	}

	public override string ExplanationPart(StatRequest req)
	{
		return "StatsReport_DifficultyMultiplier".Translate() + ": x" + Multiplier(Find.Storyteller.difficultyDef).ToStringPercent();
	}

	private float Multiplier(DifficultyDef d)
	{
		return 1f;
	}
}
