using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualObligationTargetWorker_LightBall : RitualObligationTargetWorker_ThingDef
	{
		public RitualObligationTargetWorker_LightBall()
		{
		}

		public RitualObligationTargetWorker_LightBall(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
		{
			if (!ModLister.CheckIdeology("Lightball target"))
			{
				return false;
			}
			if (!base.CanUseTargetInternal(target, obligation).canUse)
			{
				return false;
			}
			Thing thing = target.Thing;
			CompPowerTrader compPowerTrader = thing.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null)
			{
				if (compPowerTrader.PowerNet == null || !compPowerTrader.PowerNet.HasActivePowerSource)
				{
					return "RitualTargetLightBallIsNotPowered".Translate();
				}
				List<Thing> forCell = target.Map.listerBuldingOfDefInProximity.GetForCell(target.Cell, def.maxSpeakerDistance, ThingDefOf.Loudspeaker);
				bool flag = false;
				foreach (Thing item in forCell)
				{
					CompPowerTrader compPowerTrader2 = item.TryGetComp<CompPowerTrader>();
					if (item.GetRoom() == thing.GetRoom() && compPowerTrader2.PowerNet != null && compPowerTrader2.PowerNet.HasActivePowerSource)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return "RitualTargetNoPoweredSpeakers".Translate();
				}
			}
			return true;
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			yield return ThingDefOf.LightBall.label;
			yield return ThingDefOf.RitualSpot.label;
		}
	}
}
