using Verse;

namespace RimWorld;

public class RitualOutcomeComp_RolePresentNotSubstituted : RitualOutcomeComp_QualitySingleOffset
{
	public string roleId;

	public bool disableInClassicMode;

	protected override string LabelForDesc => label;

	public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
	{
		if (disableInClassicMode && Find.IdeoManager.classicMode)
		{
			return string.Empty;
		}
		return base.GetDesc(ritual, data);
	}

	public override bool Applies(LordJob_Ritual ritual)
	{
		if (disableInClassicMode && Find.IdeoManager.classicMode)
		{
			return false;
		}
		if (ritual.RoleFilled(roleId))
		{
			return !ritual.assignments.RoleSubstituted(roleId);
		}
		return false;
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		if (disableInClassicMode && Find.IdeoManager.classicMode)
		{
			return null;
		}
		bool flag = assignments.AnyPawnAssigned(roleId) && !assignments.RoleSubstituted(roleId);
		return new QualityFactor
		{
			label = LabelForDesc.CapitalizeFirst(),
			present = flag,
			qualityChange = ExpectedOffsetDesc(flag, -1f),
			quality = (flag ? qualityOffset : 0f),
			positive = flag,
			priority = 3f
		};
	}
}
