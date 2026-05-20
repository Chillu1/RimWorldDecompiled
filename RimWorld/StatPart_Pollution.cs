using Verse;

namespace RimWorld
{
	public class StatPart_Pollution : StatPart
	{
		private float multiplier = 1f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (ActiveFor(req.Thing))
			{
				val *= multiplier;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && ActiveFor(req.Thing))
			{
				return "StatsReport_Pollution".Translate() + (": x" + multiplier.ToStringPercent());
			}
			return null;
		}

		private bool ActiveFor(Thing t)
		{
			if (!ModLister.CheckBiotech("Pollution stat part"))
			{
				return false;
			}
			if (t != null && t.Spawned && t.def.deteriorateFromEnvironmentalEffects)
			{
				return t.Position.IsPolluted(t.Map);
			}
			return false;
		}
	}
}
