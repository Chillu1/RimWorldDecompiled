using UnityEngine;
using Verse;

namespace RimWorld;

public class StatWorker_MinimumHandlingSkill : StatWorker
{
	private static readonly SimpleCurve HandlingSkillFromWildness = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(0.15f, 1f),
		new CurvePoint(0.98f, 10f),
		new CurvePoint(0.99f, 14f)
	};

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		return ValueFromReq(req);
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		ThingDef thingDef = req.Def as ThingDef;
		if (thingDef.race.Humanlike)
		{
			return "StatsReport_BaseValue".Translate() + ": " + ValueFromReq(req).ToString("F0");
		}
		float f = req.Thing?.GetStatValue(StatDefOf.Wildness) ?? thingDef.GetStatValueAbstract(StatDefOf.Wildness);
		return "Wildness".Translate() + " " + f.ToStringPercent() + ": " + ValueFromReq(req).ToString("F0");
	}

	private float ValueFromReq(StatRequest req)
	{
		ThingDef thingDef = req.Def as ThingDef;
		if (thingDef.race.Humanlike)
		{
			return 0f;
		}
		float x = req.Thing?.GetStatValue(StatDefOf.Wildness) ?? thingDef.GetStatValueAbstract(StatDefOf.Wildness);
		return Mathf.Clamp(HandlingSkillFromWildness.Evaluate(x), 0f, 20f);
	}
}
