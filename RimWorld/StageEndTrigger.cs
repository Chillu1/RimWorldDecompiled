using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class StageEndTrigger : IExposable
	{
		public bool countsTowardsProgress;

		public virtual bool CountsTowardsProgress => countsTowardsProgress;

		public abstract Trigger MakeTrigger(LordJob_Ritual ritual, TargetInfo spot, IEnumerable<TargetInfo> foci, RitualStage stage);

		public virtual void ExposeData()
		{
		}
	}
}
