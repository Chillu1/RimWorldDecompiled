using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StatPart_AddedBodyPartsMass : StatPart
{
	private const float AddedBodyPartMassFactor = 0.9f;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetValue(req, out var value))
		{
			val += value;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (TryGetValue(req, out var value) && value != 0f)
		{
			return "StatsReport_AddedBodyPartsMass".Translate() + ": " + value.ToStringMassOffset();
		}
		return null;
	}

	private bool TryGetValue(StatRequest req, out float value)
	{
		return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(req, (Pawn x) => GetAddedBodyPartsMass(x), (ThingDef x) => 0f, out value);
	}

	private static float GetAddedBodyPartsMass(Pawn p)
	{
		float num = 0f;
		List<Hediff> hediffs = p.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i] is Hediff_AddedPart hediff_AddedPart && hediff_AddedPart.def.spawnThingOnRemoved != null)
			{
				num += hediff_AddedPart.def.spawnThingOnRemoved.GetStatValueAbstract(StatDefOf.Mass) * 0.9f;
			}
		}
		return num;
	}
}
