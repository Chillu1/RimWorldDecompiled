using Verse;

namespace RimWorld;

public class RitualOutcomeComp_Indoors : RitualOutcomeComp_QualitySingleOffset
{
	public override bool DataRequired => false;

	public override bool Applies(LordJob_Ritual ritual)
	{
		return true;
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		Room room = ritual.Spot.GetRoom(ritual.Map);
		if (room != null)
		{
			return (!room.PsychologicallyOutdoors) ? 1 : 0;
		}
		return 0f;
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		float quality = 0f;
		bool flag = false;
		if (ritualTarget.Map != null)
		{
			Room room = ritualTarget.Cell.GetRoom(ritualTarget.Map);
			flag = room != null && !room.PsychologicallyOutdoors;
			quality = (flag ? qualityOffset : 0f);
		}
		return new QualityFactor
		{
			label = label.CapitalizeFirst(),
			qualityChange = ExpectedOffsetDesc(positive: true, quality),
			quality = quality,
			present = flag,
			positive = true,
			priority = 0f
		};
	}
}
