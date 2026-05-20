using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_Psychophagy : PsychicRitualDef_InvocationCircle
{
	public FloatRange brainDamageRange;

	public SimpleCurve effectDurationDaysFromQualityCurve;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, parent);
		list.Add(new PsychicRitualToil_Psychophagy(InvokerRole, TargetRole, brainDamageRange));
		list.Add(new PsychicRitualToil_TargetCleanup(InvokerRole, TargetRole));
		return list;
	}

	public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		string text = Mathf.FloorToInt(effectDurationDaysFromQualityCurve.Evaluate(qualityRange.min) * 60000f).ToStringTicksToDays();
		IntRange disappearsAfterTicks = HediffDefOf.DarkPsychicShock.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks;
		FloatRange floatRange = new FloatRange(Mathf.FloorToInt(disappearsAfterTicks.min.TicksToDays()), Mathf.FloorToInt(disappearsAfterTicks.max.TicksToDays()));
		return outcomeDescription.Formatted(text, floatRange.ToString(), brainDamageRange.ToString());
	}
}
