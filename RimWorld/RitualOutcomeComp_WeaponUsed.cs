using Verse;

namespace RimWorld;

public class RitualOutcomeComp_WeaponUsed : RitualOutcomeComp_QualitySingleOffset
{
	protected override string LabelForDesc => label;

	public override bool DataRequired => false;

	public override bool Applies(LordJob_Ritual ritual)
	{
		if (ritual is LordJob_Ritual_Duel lordJob_Ritual_Duel)
		{
			return lordJob_Ritual_Duel.usedWeapon;
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
