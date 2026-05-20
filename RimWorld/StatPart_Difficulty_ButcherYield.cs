using Verse;

namespace RimWorld;

public class StatPart_Difficulty_ButcherYield : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		float num = ((Find.Storyteller != null) ? Find.Storyteller.difficulty.butcherYieldFactor : 1f);
		val *= num;
	}

	public override string ExplanationPart(StatRequest req)
	{
		return "StatsReport_DifficultyMultiplier".Translate(Find.Storyteller.difficultyDef.label) + ": " + Find.Storyteller.difficulty.butcherYieldFactor.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor);
	}
}
