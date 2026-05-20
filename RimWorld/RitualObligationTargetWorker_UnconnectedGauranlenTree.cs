using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualObligationTargetWorker_UnconnectedGauranlenTree : RitualObligationTargetFilter
	{
		public RitualObligationTargetWorker_UnconnectedGauranlenTree()
		{
		}

		public RitualObligationTargetWorker_UnconnectedGauranlenTree(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
		{
			List<Thing> trees = map.listerThings.ThingsOfDef(ThingDefOf.Plant_TreeGauranlen);
			for (int i = 0; i < trees.Count; i++)
			{
				CompTreeConnection compTreeConnection = trees[i].TryGetComp<CompTreeConnection>();
				if (compTreeConnection != null && !compTreeConnection.Connected && !compTreeConnection.ConnectionTorn)
				{
					yield return trees[i];
				}
			}
		}

		protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
		{
			Thing thing = target.Thing;
			CompTreeConnection compTreeConnection = thing.TryGetComp<CompTreeConnection>();
			if (compTreeConnection == null)
			{
				return false;
			}
			if (compTreeConnection.Connected)
			{
				return "RitualTargetConnectedGauranlenTree".Translate(thing.Named("TREE"), compTreeConnection.ConnectedPawn.Named("PAWN"));
			}
			if (compTreeConnection.ConnectionTorn)
			{
				return "RitualTargetConnectionTornGauranlenTree".Translate(thing.Named("TREE"), compTreeConnection.UntornInDurationTicks.ToStringTicksToPeriod()).Resolve().CapitalizeFirst();
			}
			return true;
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			yield return "RitualTargetUnconnectedGaruanlenTreeInfo".Translate();
		}
	}
}
