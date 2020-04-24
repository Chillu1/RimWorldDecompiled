using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StatPart_Glow : StatPart
	{
		private bool humanlikeOnly;

		private SimpleCurve factorFromGlowCurve;

		public override IEnumerable<string> ConfigErrors()
		{
			if (factorFromGlowCurve == null)
			{
				yield return "factorFromLightCurve is null.";
			}
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && ActiveFor(req.Thing))
			{
				val *= FactorFromGlow(req.Thing);
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && ActiveFor(req.Thing))
			{
				return "StatsReport_LightMultiplier".Translate(GlowLevel(req.Thing).ToStringPercent()) + ": x" + FactorFromGlow(req.Thing).ToStringPercent();
			}
			return null;
		}

		private bool ActiveFor(Thing t)
		{
			if (humanlikeOnly)
			{
				Pawn pawn = t as Pawn;
				if (pawn != null && !pawn.RaceProps.Humanlike)
				{
					return false;
				}
			}
			return t.Spawned;
		}

		private float GlowLevel(Thing t)
		{
			return t.Map.glowGrid.GameGlowAt(t.Position);
		}

		private float FactorFromGlow(Thing t)
		{
			return factorFromGlowCurve.Evaluate(GlowLevel(t));
		}
	}
}
