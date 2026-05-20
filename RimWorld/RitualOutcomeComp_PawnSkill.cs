using System;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_PawnSkill : RitualOutcomeComp_Quality
{
	[NoTranslate]
	public string roleId;

	public SkillDef skillDef;

	public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		return curve.Evaluate(Count(ritual, data));
	}

	protected float SkillValue(Pawn pawn)
	{
		return pawn.skills.GetSkill(skillDef).Level;
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		Pawn pawn = ritual.PawnWithRole(roleId);
		if (pawn == null)
		{
			return 0f;
		}
		return SkillValue(pawn);
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
		float x = SkillValue(pawn);
		float num = curve.Evaluate(x);
		string text = ((num < 0f) ? "" : "+");
		return label.CapitalizeFirst().Formatted(pawn.Named("PAWN")) + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate(text + num.ToStringPercent()) + ".";
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		Pawn pawn = assignments.FirstAssignedPawn(roleId);
		if (pawn == null)
		{
			return null;
		}
		float x = SkillValue(pawn);
		float num = curve.Evaluate(x);
		return new QualityFactor
		{
			label = label.Formatted(pawn.Named("PAWN")),
			count = x.ToString(),
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
