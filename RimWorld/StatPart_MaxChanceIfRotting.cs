using Verse;

namespace RimWorld
{
	public class StatPart_MaxChanceIfRotting : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (IsRotting(req))
			{
				val = 1f;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (IsRotting(req))
			{
				return "StatsReport_NotFresh".Translate() + ": " + 1f.ToStringPercent();
			}
			return null;
		}

		private bool IsRotting(StatRequest req)
		{
			if (!req.HasThing)
			{
				return false;
			}
			return req.Thing.GetRotStage() != RotStage.Fresh;
		}
	}
}
