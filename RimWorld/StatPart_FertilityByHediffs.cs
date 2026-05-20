using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class StatPart_FertilityByHediffs : StatPart
{
	public override string ExplanationPart(StatRequest req)
	{
		if (!(req.Thing is Pawn pawn))
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder(32);
		Factor(pawn, stringBuilder);
		return stringBuilder.ToString();
	}

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.Thing is Pawn pawn)
		{
			val *= Factor(pawn);
		}
	}

	public static float Factor(Pawn pawn, StringBuilder explanation = null)
	{
		float num = 1f;
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			HediffStage curStage = hediff.CurStage;
			if (curStage != null && curStage.fertilityFactor != 1f)
			{
				num *= curStage.fertilityFactor;
				explanation?.AppendLine(hediff.LabelBaseCap + ": x" + curStage.fertilityFactor.ToStringPercent());
			}
		}
		return Mathf.Max(num, 0f);
	}
}
