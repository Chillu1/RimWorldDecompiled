using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_SkipAbductionPlayer : PsychicRitualDef_InvocationCircle
{
	public SimpleCurve comaDurationDaysFromQualityCurve;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, parent);
		list.Add(new PsychicRitualToil_SkipAbductionPlayer(InvokerRole));
		return list;
	}

	public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		string text = Mathf.FloorToInt(comaDurationDaysFromQualityCurve.Evaluate(qualityRange.min) * 60000f).ToStringTicksToDays();
		return outcomeDescription.Formatted(text);
	}
}
