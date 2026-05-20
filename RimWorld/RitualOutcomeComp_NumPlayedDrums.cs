using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_NumPlayedDrums : RitualOutcomeComp_Quality
{
	public int maxDistance;

	public override RitualOutcomeComp_Data MakeData()
	{
		return new RitualOutcomeComp_DataThingPresence();
	}

	public override void Tick(LordJob_Ritual ritual, RitualOutcomeComp_Data data, float progressAmount)
	{
		base.Tick(ritual, data, progressAmount);
		TargetInfo selectedTarget = ritual.selectedTarget;
		if (selectedTarget.ThingDestroyed || !selectedTarget.HasThing)
		{
			return;
		}
		RitualOutcomeComp_DataThingPresence ritualOutcomeComp_DataThingPresence = (RitualOutcomeComp_DataThingPresence)data;
		foreach (Thing item in selectedTarget.Map.listerBuldingOfDefInProximity.GetForCell(selectedTarget.Cell, maxDistance, ThingDefOf.Drum))
		{
			if (item is Building_MusicalInstrument building_MusicalInstrument && item.GetRoom() == selectedTarget.Cell.GetRoom(selectedTarget.Map) && building_MusicalInstrument.IsBeingPlayed)
			{
				if (!ritualOutcomeComp_DataThingPresence.presentForTicks.ContainsKey(item))
				{
					ritualOutcomeComp_DataThingPresence.presentForTicks.Add(item, 0f);
				}
				ritualOutcomeComp_DataThingPresence.presentForTicks[item]++;
			}
		}
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		int num = 0;
		foreach (KeyValuePair<Thing, float> presentForTick in ((RitualOutcomeComp_DataThingPresence)data).presentForTicks)
		{
			if (presentForTick.Value >= (float)(ritual.DurationTicks / 2))
			{
				num++;
			}
		}
		return (int)Math.Min(num, curve.Points[curve.PointsCount - 1].x);
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		int count = ritualTarget.Map.listerBuldingOfDefInProximity.GetForCell(ritualTarget.Cell, maxDistance, ThingDefOf.Drum).Count;
		float quality = curve.Evaluate(count);
		return new QualityFactor
		{
			label = "RitualPredictedOutcomeDescNumDrums".Translate(),
			count = Mathf.Min(count, base.MaxValue) + " / " + base.MaxValue,
			qualityChange = ExpectedOffsetDesc(positive: true, quality),
			quality = quality,
			positive = (count > 0),
			priority = 1f
		};
	}
}
