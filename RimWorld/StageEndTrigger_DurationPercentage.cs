using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class StageEndTrigger_DurationPercentage : StageEndTrigger
	{
		public float percentage;

		public override bool CountsTowardsProgress => true;

		public override Trigger MakeTrigger(LordJob_Ritual ritual, TargetInfo spot, IEnumerable<TargetInfo> foci, RitualStage stage)
		{
			return new Trigger_TicksPassedRitual(Mathf.RoundToInt((float)ritual.DurationTicks * percentage), stage);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref percentage, "percentage", 0f);
		}
	}
}
