using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_Brainwipe : PsychicRitualDef_InvocationCircle
{
	public SimpleCurve comaDurationDaysFromQualityCurve;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, parent);
		list.Add(new PsychicRitualToil_Brainwipe(InvokerRole, TargetRole));
		list.Add(new PsychicRitualToil_TargetCleanup(InvokerRole, TargetRole));
		return list;
	}

	public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		return outcomeDescription.Formatted(Mathf.FloorToInt(comaDurationDaysFromQualityCurve.Evaluate(qualityRange.min) * 60000f).ToStringTicksToDays());
	}

	public override IEnumerable<string> GetPawnTooltipExtras(Pawn pawn)
	{
		if (pawn.guest != null && !pawn.guest.Recruitable)
		{
			yield return "NonRecruitable".Translate();
		}
	}
}
