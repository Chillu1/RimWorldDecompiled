using Verse;

namespace RimWorld
{
	public class StatPart_HasRelic : StatPart
	{
		public float offset;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && ActiveFor(req.Thing))
			{
				val += offset;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && ActiveFor(req.Thing))
			{
				return "StatsReports_HasRelic".Translate() + ": " + offset.ToStringWithSign();
			}
			return null;
		}

		private bool ActiveFor(Thing thing)
		{
			CompRelicContainer compRelicContainer = thing.TryGetComp<CompRelicContainer>();
			if (compRelicContainer != null)
			{
				return compRelicContainer.ContainedThing != null;
			}
			return false;
		}
	}
}
