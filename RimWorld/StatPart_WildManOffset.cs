using Verse;

namespace RimWorld
{
	public class StatPart_WildManOffset : StatPart
	{
		public float offset;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (IsWildMan(req))
			{
				val += offset;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (IsWildMan(req))
			{
				return "StatsReport_WildMan".Translate() + ": " + offset.ToStringWithSign();
			}
			return null;
		}

		private bool IsWildMan(StatRequest req)
		{
			return (req.Thing as Pawn)?.IsWildMan() ?? false;
		}
	}
}
