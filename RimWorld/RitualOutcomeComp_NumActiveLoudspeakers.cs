using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_NumActiveLoudspeakers : RitualOutcomeComp_Quality
{
	public int maxDistance;

	public override RitualOutcomeComp_Data MakeData()
	{
		return new RitualOutcomeComp_DataThingPresence();
	}

	private bool LoudspeakerActive(Thing s, TargetInfo target)
	{
		if (s.GetRoom() == target.Cell.GetRoom(target.Map))
		{
			return s.TryGetComp<CompPowerTrader>().PowerOn;
		}
		return false;
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
		foreach (Thing item in selectedTarget.Map.listerBuldingOfDefInProximity.GetForCell(selectedTarget.Cell, maxDistance, ThingDefOf.Loudspeaker))
		{
			if (LoudspeakerActive(item, selectedTarget))
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
		List<Thing> forCell = ritualTarget.Map.listerBuldingOfDefInProximity.GetForCell(ritualTarget.Cell, maxDistance, ThingDefOf.Loudspeaker);
		int num = 0;
		foreach (Thing item in forCell)
		{
			CompPowerTrader compPowerTrader = item.TryGetComp<CompPowerTrader>();
			CompFlickable compFlickable = item.TryGetComp<CompFlickable>();
			if (compPowerTrader != null && compPowerTrader.PowerNet != null && compPowerTrader.PowerNet.HasActivePowerSource && compFlickable != null && compFlickable.SwitchIsOn)
			{
				num++;
			}
		}
		float quality = curve.Evaluate(num);
		return new QualityFactor
		{
			label = "RitualPredictedOutcomeDescNumActiveLoudspeakers".Translate(),
			count = Mathf.Min(num, base.MaxValue) + " / " + base.MaxValue,
			qualityChange = ExpectedOffsetDesc(positive: true, quality),
			quality = quality,
			positive = (num > 0),
			priority = 1f
		};
	}
}
