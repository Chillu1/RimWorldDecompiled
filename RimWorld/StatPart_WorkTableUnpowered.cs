using Verse;

namespace RimWorld
{
	public class StatPart_WorkTableUnpowered : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && Applies(req.Thing))
			{
				val *= req.Thing.def.building.unpoweredWorkTableWorkSpeedFactor;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && Applies(req.Thing))
			{
				float unpoweredWorkTableWorkSpeedFactor = req.Thing.def.building.unpoweredWorkTableWorkSpeedFactor;
				return "NoPower".Translate() + ": x" + unpoweredWorkTableWorkSpeedFactor.ToStringPercent();
			}
			return null;
		}

		public static bool Applies(Thing th)
		{
			if (th.def.building.unpoweredWorkTableWorkSpeedFactor == 0f)
			{
				return false;
			}
			CompPowerTrader compPowerTrader = th.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null && !compPowerTrader.PowerOn)
			{
				return true;
			}
			return false;
		}
	}
}
