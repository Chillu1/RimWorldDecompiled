using Verse;

namespace RimWorld;

public class RitualOutcomeComp_TargetThingStat : RitualOutcomeComp_Quality
{
	public StatDef statDef;

	public bool mustBeBed;

	public override bool DataRequired => false;

	public override bool Applies(LordJob_Ritual ritual)
	{
		if (ritual.selectedTarget.HasThing)
		{
			if (mustBeBed)
			{
				return ritual.selectedTarget.Thing.def.IsBed;
			}
			return true;
		}
		return false;
	}

	private float StatValue(TargetInfo ritualTarget)
	{
		if (ritualTarget.HasThing)
		{
			return ritualTarget.Thing.GetStatValue(statDef);
		}
		return 0f;
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		return StatValue(ritual.selectedTarget);
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		float x = StatValue(ritualTarget);
		float quality = curve.Evaluate(x);
		return new QualityFactor
		{
			label = label.CapitalizeFirst(),
			count = x.ToString(),
			present = (!mustBeBed || (ritualTarget.HasThing && ritualTarget.Thing.def.IsBed)),
			qualityChange = ExpectedOffsetDesc(positive: true, quality),
			quality = quality,
			positive = true,
			priority = 0f
		};
	}
}
