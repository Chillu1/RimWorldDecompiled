using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class RitualObligationTargetWorker_AnimaTree : RitualObligationTargetFilter
	{
		public RitualObligationTargetWorker_AnimaTree()
		{
		}

		public RitualObligationTargetWorker_AnimaTree(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
		{
			return Enumerable.Empty<TargetInfo>();
		}

		protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
		{
			CompPsylinkable compPsylinkable = target.Thing.TryGetComp<CompPsylinkable>();
			if (compPsylinkable == null)
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			foreach (Pawn item in target.Map.mapPawns.FreeColonistsSpawned)
			{
				if ((bool)compPsylinkable.CanPsylink(item, null, checkSpot: false))
				{
					flag2 = true;
				}
				if (compPsylinkable.Props.requiredFocus.CanPawnUse(item))
				{
					flag = true;
				}
			}
			if (compPsylinkable.CompSubplant.SubplantsForReading.Count < compPsylinkable.Props.requiredSubplantCountPerPsylinkLevel[0])
			{
				return "RitualTargetAnimaTreeNotEnoughAnimaGrass".Translate(compPsylinkable.Props.requiredSubplantCountPerPsylinkLevel[0]);
			}
			if (!flag)
			{
				return "RitualTargetAnimaTreeNoPawnsWithNatureFocus".Translate();
			}
			if (!flag2)
			{
				return "RitualTargetAnimaTreeNoPawnsToLink".Translate();
			}
			return true;
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			yield return "RitualTargetAnimaTreeInfo".Translate();
		}
	}
}
