using Verse;

namespace RimWorld
{
	public class StatPart_Health : StatPart_Curve
	{
		protected override bool AppliesTo(StatRequest req)
		{
			if (req.HasThing && req.Thing.def.useHitPoints)
			{
				return req.Thing.def.healthAffectsPrice;
			}
			return false;
		}

		protected override float CurveXGetter(StatRequest req)
		{
			return (float)req.Thing.HitPoints / (float)req.Thing.MaxHitPoints;
		}

		protected override string ExplanationLabel(StatRequest req)
		{
			return "StatsReport_HealthMultiplier".Translate(req.Thing.HitPoints + " / " + req.Thing.MaxHitPoints);
		}
	}
}
