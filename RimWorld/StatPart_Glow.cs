using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StatPart_Glow : StatPart
{
	private bool pawnOnly;

	private bool humanlikeOnly;

	private SimpleCurve factorFromGlowCurve;

	private bool ignoreIfPrefersDarkness;

	private bool ignoreIfIncapableOfSight;

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
		if (t is Pawn pawn)
		{
			if (humanlikeOnly && !pawn.RaceProps.Humanlike)
			{
				return false;
			}
			if (pawn.IsShambler)
			{
				return false;
			}
			if (ignoreIfIncapableOfSight && PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn))
			{
				return false;
			}
			if (ignoreIfPrefersDarkness)
			{
				if (pawn.Ideo != null && pawn.Ideo.IdeoPrefersDarkness())
				{
					return false;
				}
				if (pawn.genes != null && !pawn.genes.AffectedByDarkness)
				{
					return false;
				}
			}
		}
		else if (pawnOnly)
		{
			return false;
		}
		return t.Spawned;
	}

	private float GlowLevel(Thing t)
	{
		return t.Map.glowGrid.GroundGlowAt(t.Position);
	}

	private float FactorFromGlow(Thing t)
	{
		return factorFromGlowCurve.Evaluate(GlowLevel(t));
	}
}
