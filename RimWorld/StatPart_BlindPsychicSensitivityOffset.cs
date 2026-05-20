using UnityEngine;
using Verse;

namespace RimWorld;

public class StatPart_BlindPsychicSensitivityOffset : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetPsychicOffset(req.Thing, out var offset))
		{
			val += offset;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && TryGetPsychicOffset(req.Thing, out var offset))
		{
			return "StatsReport_BlindPsychicSensitivityOffset".Translate() + (": +" + offset.ToStringPercent());
		}
		return null;
	}

	private bool TryGetPsychicOffset(Thing t, out float offset)
	{
		offset = 0f;
		if (!(t is Pawn pawn))
		{
			return false;
		}
		if (!ConsideredBlind(pawn) || pawn.Ideo == null)
		{
			return false;
		}
		foreach (Precept item in pawn.Ideo.PreceptsListForReading)
		{
			offset += item.def.blindPsychicSensitivityOffset;
		}
		return !Mathf.Approximately(offset, 0f);
	}

	private bool ConsideredBlind(Pawn pawn)
	{
		foreach (BodyPartRecord item in pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.SightSource))
		{
			if (!pawn.health.hediffSet.PartIsMissing(item))
			{
				return false;
			}
		}
		return true;
	}
}
