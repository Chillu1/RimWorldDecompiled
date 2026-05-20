using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_BloodRain : PsychicRitualDef_InvocationCircle
{
	private FloatRange durationHoursFromQualityRange;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
		list.Add(new PsychicRitualToil_BloodRain(InvokerRole, durationHoursFromQualityRange));
		return list;
	}

	public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		return outcomeDescription.Formatted(Mathf.FloorToInt(durationHoursFromQualityRange.LerpThroughRange(qualityRange.min) * 2500f).ToStringTicksToPeriod());
	}
}
