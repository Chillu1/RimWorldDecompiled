using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class RitualTargetFilter_SelectedThing : RitualTargetFilter
	{
		public RitualTargetFilter_SelectedThing()
		{
		}

		public RitualTargetFilter_SelectedThing(RitualTargetFilterDef def)
			: base(def)
		{
		}

		public override bool CanStart(TargetInfo initiator, TargetInfo selectedTarget, out string rejectionReason)
		{
			rejectionReason = null;
			return true;
		}

		public override TargetInfo BestTarget(TargetInfo initiator, TargetInfo selectedTarget)
		{
			return selectedTarget;
		}

		public override IEnumerable<string> GetTargetInfos(TargetInfo initiator)
		{
			return Enumerable.Empty<string>();
		}
	}
}
