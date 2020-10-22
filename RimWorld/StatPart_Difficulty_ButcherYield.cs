using Verse;

namespace RimWorld
{
	public class StatPart_Difficulty_ButcherYield : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			float num = ((Find.Storyteller != null) ? Find.Storyteller.difficultyValues.butcherYieldFactor : 1f);
			val *= num;
		}

		public override string ExplanationPart(StatRequest req)
		{
			return "StatsReport_DifficultyMultiplier".Translate(Find.Storyteller.difficulty.label) + ": " + Find.Storyteller.difficultyValues.butcherYieldFactor.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor);
		}
	}
}
