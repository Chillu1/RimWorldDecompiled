using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class RitualTargetFilter : IExposable
	{
		public RitualTargetFilterDef def;

		public RitualTargetFilter()
		{
		}

		public RitualTargetFilter(RitualTargetFilterDef def)
		{
			this.def = def;
		}

		public abstract bool CanStart(TargetInfo initiator, TargetInfo selectedTarget, out string rejectionReason);

		public abstract TargetInfo BestTarget(TargetInfo initiator, TargetInfo selectedTarget);

		public abstract IEnumerable<string> GetTargetInfos(TargetInfo initiator);

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
		}
	}
}
