using Verse;

namespace RimWorld
{
	public class StatPart_IsCorpseFresh : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (TryGetIsFreshFactor(req, out var factor))
			{
				val *= factor;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (TryGetIsFreshFactor(req, out var factor) && factor != 1f)
			{
				return "StatsReport_NotFresh".Translate() + ": x" + factor.ToStringPercent();
			}
			return null;
		}

		private bool TryGetIsFreshFactor(StatRequest req, out float factor)
		{
			if (!req.HasThing)
			{
				factor = 1f;
				return false;
			}
			Corpse corpse = req.Thing as Corpse;
			if (corpse == null)
			{
				factor = 1f;
				return false;
			}
			factor = ((corpse.GetRotStage() == RotStage.Fresh) ? 1f : 0f);
			return true;
		}
	}
}
