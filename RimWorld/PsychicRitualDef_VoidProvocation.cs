using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_VoidProvocation : PsychicRitualDef_InvocationCircle
{
	public SimpleCurve psychicShockChanceFromQualityCurve;

	public FloatRange darkPsychicShockDurarionHoursRange;

	public FloatRange incidentDelayHoursRange;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
		list.Add(new PsychicRitualToil_VoidProvocation(InvokerRole, psychicShockChanceFromQualityCurve));
		return list;
	}

	public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		return outcomeDescription.Formatted(psychicShockChanceFromQualityCurve.Evaluate(qualityRange.min).ToStringPercent());
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
	}
}
