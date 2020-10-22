using Verse;

namespace RimWorld
{
	public class StatPart_Difficulty_MiningYield : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			val *= Find.Storyteller.difficultyValues.mineYieldFactor;
		}

		public override string ExplanationPart(StatRequest req)
		{
			return "StatsReport_DifficultyMultiplier".Translate(Find.Storyteller.difficulty.label) + ": " + Find.Storyteller.difficultyValues.mineYieldFactor.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor);
		}
	}
}
