using System.Collections.Generic;

namespace RimWorld
{
	public abstract class RitualStageTickActionMaker
	{
		public abstract IEnumerable<ActionOnTick> GenerateTimedActions(LordJob_Ritual ritual, RitualStage stage);
	}
}
