using Verse;

namespace RimWorld;

public class RitualOutcomeComp_DuelistDied : RitualOutcomeComp_QualitySingleOffset
{
	protected override string LabelForDesc => label;

	public override bool DataRequired => false;

	public override bool Applies(LordJob_Ritual ritual)
	{
		if (ritual is LordJob_Ritual_Duel lordJob_Ritual_Duel)
		{
			return lordJob_Ritual_Duel.duelists.Any((Pawn d) => d.Dead);
		}
		return false;
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		return new QualityFactor
		{
			label = LabelForDesc.CapitalizeFirst(),
			present = false,
			uncertainOutcome = true,
			qualityChange = ExpectedOffsetDesc(positive: true, -1f),
			quality = qualityOffset,
			positive = true
		};
	}
}
