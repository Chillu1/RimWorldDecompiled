using Verse;

namespace RimWorld;

public class RitualOutcomeComp_PawnAge : RitualOutcomeComp_QualitySingleOffset
{
	[NoTranslate]
	public string roleId;

	public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		return curve.Evaluate(Count(ritual, data));
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		return ritual.PawnWithRole(roleId)?.ageTracker.AgeBiologicalYearsFloat ?? 0f;
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
		float num = curve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
		string text = ((num < 0f) ? "" : "+");
		return LabelForDesc.Formatted(pawn.Named("PAWN")) + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate(text + num.ToStringPercent()) + ".";
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		Pawn pawn = assignments.FirstAssignedPawn(roleId);
		if (pawn == null)
		{
			return null;
		}
		float num = curve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
		return new QualityFactor
		{
			label = label.Formatted(pawn.Named("PAWN")),
			count = pawn.ageTracker.AgeBiologicalYears.ToString(),
			qualityChange = ExpectedOffsetDesc(num > 0f, num),
			positive = (num > 0f),
			quality = num,
			priority = 0f
		};
	}

	public override bool Applies(LordJob_Ritual ritual)
	{
		return true;
	}
}
