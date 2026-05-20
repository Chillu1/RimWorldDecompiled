using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_NumParticipantsWithTag : RitualOutcomeComp_Quality
{
	public string tag;

	public override bool DataRequired => false;

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		int num = 0;
		foreach (Pawn participant in ritual.assignments.Participants)
		{
			if (ritual.PawnTagSet(participant, tag))
			{
				num++;
			}
		}
		return num;
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		int num = assignments.Participants.Count((Pawn p) => p.RaceProps.Humanlike);
		float quality = curve.Evaluate(num);
		return new QualityFactor
		{
			label = label.CapitalizeFirst(),
			count = Mathf.Min(num, base.MaxValue) + " / " + base.MaxValue,
			qualityChange = ExpectedOffsetDesc(num > 0, quality),
			quality = quality,
			positive = (num > 0),
			priority = 4f
		};
	}
}
