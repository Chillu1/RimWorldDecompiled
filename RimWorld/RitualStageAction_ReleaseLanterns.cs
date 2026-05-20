using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualStageAction_ReleaseLanterns : RitualStageTickActionMaker
	{
		public int woodCost = 4;

		public IntRange preliminaryTicks;

		public override IEnumerable<ActionOnTick> GenerateTimedActions(LordJob_Ritual ritual, RitualStage stage)
		{
			StageEndTrigger_DurationPercentage stageEndTrigger_DurationPercentage = (StageEndTrigger_DurationPercentage)stage.endTriggers.FirstOrFallback((StageEndTrigger e) => e is StageEndTrigger_DurationPercentage);
			if (stageEndTrigger_DurationPercentage == null)
			{
				yield break;
			}
			int durationTicks = (int)(stageEndTrigger_DurationPercentage.percentage * (float)ritual.DurationTicks);
			foreach (Pawn participant in ritual.assignments.Participants)
			{
				yield return new ActionOnTick_ReleaseLantern
				{
					tick = durationTicks - preliminaryTicks.RandomInRange,
					pawn = participant,
					woodCost = woodCost
				};
			}
		}
	}
}
