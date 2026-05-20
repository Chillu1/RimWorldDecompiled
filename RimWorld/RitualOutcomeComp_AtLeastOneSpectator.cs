using Verse;

namespace RimWorld;

public class RitualOutcomeComp_AtLeastOneSpectator : RitualOutcomeComp_ParticipantCount
{
	[MustTranslate]
	public string labelNotMet;

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		int num = assignments.Participants.Count((Pawn p) => Counts(assignments, p));
		float quality = ((num > 0) ? qualityOffset : 0f);
		return new QualityFactor
		{
			label = label.CapitalizeFirst(),
			present = (num > 0),
			qualityChange = ExpectedOffsetDesc(positive: true, quality),
			quality = quality,
			positive = true,
			priority = 4f
		};
	}

	public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
	{
		return ((Count(ritual, data) > 0f) ? label : labelNotMet).CapitalizeFirst().Formatted() + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate(qualityOffset.ToStringPercent()) + ".";
	}

	public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		if (!(Count(ritual, data) > 0f))
		{
			return 0f;
		}
		return qualityOffset;
	}

	protected override string ExpectedOffsetDesc(bool positive, float quality = 0f)
	{
		if (!positive)
		{
			return "";
		}
		return quality.ToStringWithSign("0.#%");
	}
}
