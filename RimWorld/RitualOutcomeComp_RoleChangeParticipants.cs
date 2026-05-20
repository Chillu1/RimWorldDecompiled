using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_RoleChangeParticipants : RitualOutcomeComp_ParticipantCount
{
	public override RitualOutcomeComp_Data MakeData()
	{
		return new RitualOutcomeComp_DataRoleChangeParticipants();
	}

	public override void Notify_AssignmentsChanged(RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		int num = 0;
		Pawn pawn = assignments.FirstAssignedPawn("role_changer");
		if (pawn == null)
		{
			return;
		}
		foreach (Pawn item in pawn.Map.mapPawns.FreeColonistsSpawned)
		{
			if (item != pawn && item.Ideo == pawn.Ideo && !item.Downed)
			{
				num++;
			}
		}
		((RitualOutcomeComp_DataRoleChangeParticipants)data).desiredParticipantCount = num / 2;
	}

	protected override string ExpectedOffsetDesc(bool positive, float quality = 0f)
	{
		return quality.ToStringPercent();
	}

	public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		int desiredParticipantCount = ((RitualOutcomeComp_DataRoleChangeParticipants)data).desiredParticipantCount;
		return ((int)Count(ritual, data) >= desiredParticipantCount) ? 1 : 0;
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		int num = 0;
		foreach (KeyValuePair<Thing, float> presentForTick in ((RitualOutcomeComp_DataThingPresence)data).presentForTicks)
		{
			Pawn p = (Pawn)presentForTick.Key;
			if (Counts(ritual.assignments, p) && presentForTick.Value >= (float)ritual.DurationTicks / 2f)
			{
				num++;
			}
		}
		return num;
	}

	public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
	{
		int desiredParticipantCount = ((RitualOutcomeComp_DataRoleChangeParticipants)data).desiredParticipantCount;
		return Count(ritual, data) + " / " + desiredParticipantCount + " " + label + ": +" + QualityOffset(ritual, data).ToStringPercent();
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		int desiredParticipantCount = ((RitualOutcomeComp_DataRoleChangeParticipants)data).desiredParticipantCount;
		int num = assignments.Participants.Count((Pawn p) => Counts(assignments, p));
		bool flag = num >= desiredParticipantCount;
		int num2 = (flag ? 1 : 0);
		return new QualityFactor
		{
			label = label.CapitalizeFirst(),
			count = num + " / " + desiredParticipantCount,
			qualityChange = ExpectedOffsetDesc(flag, num2),
			quality = num2,
			positive = flag,
			priority = 4f
		};
	}
}
