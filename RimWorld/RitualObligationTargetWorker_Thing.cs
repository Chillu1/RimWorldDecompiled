using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualObligationTargetWorker_Thing : RitualObligationTargetFilter
	{
		public RitualObligationTargetWorker_Thing()
		{
		}

		public RitualObligationTargetWorker_Thing(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
		{
			if (obligation.targetA.HasThing)
			{
				yield return obligation.targetA.Thing;
			}
		}

		protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
		{
			return target.HasThing && target.Thing == obligation.targetA.Thing;
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			yield return obligation.targetA.Thing.LabelShort;
		}
	}
}
