using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_Chronophagy : PsychicRitualDef_InvocationCircle
{
	public FloatRange yearsTransferredFromQualityRange;

	public SimpleCurve brainDamageFromAgeCurve;

	public const int MinimumAge = 13;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, parent);
		list.Add(new PsychicRitualToil_Chronophagy(InvokerRole, TargetRole, yearsTransferredFromQualityRange));
		list.Add(new PsychicRitualToil_TargetCleanup(InvokerRole, TargetRole));
		return list;
	}

	public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
	{
		foreach (string item in base.BlockingIssues(assignments, map))
		{
			yield return item;
		}
		if (assignments.FirstAssignedPawn(invokerRole).ageTracker.AgeBiologicalYears < 13)
		{
			yield return "InvokerTooYoung".Translate(13);
		}
		if (assignments.FirstAssignedPawn(targetRole).ageTracker.AgeBiologicalYears < 13)
		{
			yield return "PsychicRitualTargetTooYoung".Translate(13);
		}
	}

	public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		float num = yearsTransferredFromQualityRange.LerpThroughRange(qualityRange.min);
		Pawn pawn = assignments.FirstAssignedPawn(invokerRole);
		Pawn pawn2 = assignments.FirstAssignedPawn(targetRole);
		if (pawn != null)
		{
			float num2 = Mathf.Max(pawn.ageTracker.AgeBiologicalYearsFloat - num, 13f);
			float num3 = pawn.ageTracker.AgeBiologicalYearsFloat - num2;
			TaggedString result = outcomeDescription.Formatted(num3, yearsTransferredFromQualityRange.LerpThroughRange(qualityRange.min));
			if (pawn2 != null)
			{
				result += "\n\n" + "CronophagyAgeChange".Translate(pawn.Named("INVOKER"), pawn.ageTracker.AgeBiologicalYearsFloat - num3, pawn2.Named("TARGET"), pawn2.ageTracker.AgeBiologicalYearsFloat + yearsTransferredFromQualityRange.LerpThroughRange(qualityRange.min));
			}
			return result;
		}
		return outcomeDescription.Formatted(num, num);
	}

	public override IEnumerable<string> GetPawnTooltipExtras(Pawn pawn)
	{
		yield return string.Concat("Stat_Age_Label".Translate() + ": ", pawn.ageTracker.AgeBiologicalYears.ToString());
	}
}
