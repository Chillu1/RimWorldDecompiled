using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_NeurosisPulse : PsychicRitualDef_InvocationCircle
{
	public SimpleCurve durationDaysFromQualityCurve;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
		list.Add(new PsychicRitualToil_NeurosisPulse(InvokerRole));
		return list;
	}

	public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		return outcomeDescription.Formatted(Mathf.FloorToInt(durationDaysFromQualityCurve.Evaluate(qualityRange.min) * 60000f).ToStringTicksToDays());
	}
}
