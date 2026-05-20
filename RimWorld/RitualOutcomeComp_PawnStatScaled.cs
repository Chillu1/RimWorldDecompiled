using System;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_PawnStatScaled : RitualOutcomeComp_QualitySingleOffset
{
	[NoTranslate]
	public string roleId;

	public StatDef statDef;

	public float scaledBy = 1f;

	public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		return Count(ritual, data);
	}

	protected float StatValue(Pawn pawn)
	{
		if (curve == null)
		{
			return pawn.GetStatValue(statDef);
		}
		return curve.Evaluate(pawn.GetStatValue(statDef));
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		Pawn pawn = ritual.PawnWithRole(roleId);
		if (pawn == null)
		{
			return 0f;
		}
		if (statDef.Worker.IsDisabledFor(pawn))
		{
			return 0f;
		}
		return StatValue(pawn) * scaledBy;
	}

	public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
	{
		if (ritual == null)
		{
			return labelAbstract;
		}
		Pawn pawn = ritual?.PawnWithRole(roleId);
		if (pawn == null)
		{
			return null;
		}
		float num = 0f;
		if (!statDef.Worker.IsDisabledFor(pawn))
		{
			num = StatValue(pawn);
		}
		float num2 = num * scaledBy;
		string text = ((num2 < 0f) ? "" : "+");
		return LabelForDesc.Formatted(pawn.Named("PAWN")) + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate(text + num2.ToStringPercent()) + ".";
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		Pawn pawn = assignments.FirstAssignedPawn(roleId);
		if (pawn == null)
		{
			return null;
		}
		float f = 0f;
		float num = 0f;
		if (!statDef.Worker.IsDisabledFor(pawn))
		{
			f = pawn.GetStatValue(statDef);
			num = StatValue(pawn) * scaledBy;
		}
		return new QualityFactor
		{
			label = label.Formatted(pawn.Named("PAWN")),
			count = f.ToStringPercent(),
			qualityChange = ((Math.Abs(num) > float.Epsilon) ? "OutcomeBonusDesc_QualitySingleOffset".Translate(num.ToStringWithSign("0.#%")).Resolve() : " - "),
			positive = (num >= 0f),
			quality = num,
			priority = 0f
		};
	}

	public override bool Applies(LordJob_Ritual ritual)
	{
		return true;
	}
}
