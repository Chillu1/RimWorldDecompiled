using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_ParticipantCount : RitualOutcomeComp_Quality
{
	public override RitualOutcomeComp_Data MakeData()
	{
		return new RitualOutcomeComp_DataThingPresence();
	}

	public override void Tick(LordJob_Ritual ritual, RitualOutcomeComp_Data data, float progressAmount)
	{
		base.Tick(ritual, data, progressAmount);
		RitualOutcomeComp_DataThingPresence ritualOutcomeComp_DataThingPresence = (RitualOutcomeComp_DataThingPresence)data;
		foreach (Pawn item in ritual.PawnsToCountTowardsPresence)
		{
			if (ritual.Ritual != null)
			{
				RitualRole ritualRole = ritual.RoleFor(item, includeForced: true);
				if (ritualRole != null && !ritualRole.countsAsParticipant)
				{
					continue;
				}
			}
			if (GatheringsUtility.InGatheringArea(item.Position, ritual.Spot, item.MapHeld))
			{
				if (!ritualOutcomeComp_DataThingPresence.presentForTicks.ContainsKey(item))
				{
					ritualOutcomeComp_DataThingPresence.presentForTicks.Add(item, 0f);
				}
				ritualOutcomeComp_DataThingPresence.presentForTicks[item] += progressAmount;
			}
		}
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		int num = 0;
		RitualOutcomeComp_DataThingPresence obj = (RitualOutcomeComp_DataThingPresence)data;
		float num2 = ((ritual.DurationTicks != 0) ? ((float)ritual.DurationTicks) : ritual.TicksPassedWithProgress);
		foreach (KeyValuePair<Thing, float> presentForTick in obj.presentForTicks)
		{
			Pawn p = (Pawn)presentForTick.Key;
			if (Counts(ritual.assignments, p) && presentForTick.Value >= num2 / 2f)
			{
				num++;
			}
		}
		return (curve != null) ? ((int)Math.Min(num, curve.Points[curve.PointsCount - 1].x)) : num;
	}

	protected bool Counts(RitualRoleAssignments assignments, Pawn p)
	{
		if (assignments != null && assignments.Ritual == null && assignments.Required(p))
		{
			return false;
		}
		RitualRole ritualRole = assignments?.RoleForPawn(p);
		if (ritualRole != null && !ritualRole.countsAsParticipant)
		{
			return false;
		}
		return p.RaceProps.Humanlike;
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		int num = assignments.Participants.Count((Pawn p) => Counts(assignments, p));
		float quality = curve.Evaluate(num);
		return new QualityFactor
		{
			label = label.CapitalizeFirst(),
			count = num + " / " + Mathf.Max(base.MaxValue, num),
			qualityChange = ExpectedOffsetDesc(positive: true, quality),
			quality = quality,
			positive = true,
			priority = 4f
		};
	}
}
