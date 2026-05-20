using Verse;

namespace RimWorld
{
	public class StatPart_ToxicFallout : StatPart
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
				return "StatsReport_MultiplierFor".Translate(GameConditionDefOf.ToxicFallout.label) + (": x" + multiplier.ToStringPercent());
			}
			return null;
		}

		private bool ActiveFor(Thing t)
		{
			if (t != null && t.def.deteriorateFromEnvironmentalEffects && t.MapHeld != null && t.MapHeld.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && t.PositionHeld.IsValid)
			{
				return !t.PositionHeld.Roofed(t.MapHeld);
			}
			return false;
		}
	}
}
