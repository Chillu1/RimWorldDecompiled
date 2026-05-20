using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_Philophagy : PsychicRitualDef_InvocationCircle
{
	public FloatRange brainDamageRange;

	public SimpleCurve xpTransferFromQualityCurve;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, parent);
		list.Add(new PsychicRitualToil_Philophagy(InvokerRole, TargetRole, brainDamageRange));
		list.Add(new PsychicRitualToil_TargetCleanup(InvokerRole, TargetRole));
		return list;
	}

	public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		float num = xpTransferFromQualityCurve.Evaluate(qualityRange.min);
		string text = num.ToStringPercent();
		Pawn pawn = assignments.FirstAssignedPawn(InvokerRole);
		Pawn pawn2 = assignments.FirstAssignedPawn(TargetRole);
		TaggedString result = outcomeDescription.Formatted(text);
		if (pawn == null || pawn2 == null)
		{
			return result;
		}
		float xpTransfer;
		SkillDef philophagySkillAndXpTransfer = PsychicRitualUtility.GetPhilophagySkillAndXpTransfer(pawn, pawn2, num, out xpTransfer);
		if (philophagySkillAndXpTransfer != null)
		{
			result += "\n\n" + "PhilophagyTransferReport".Translate(pawn.Named("INVOKER"), xpTransfer, philophagySkillAndXpTransfer.Named("SKILL"), pawn2.Named("TARGET"));
		}
		return result;
	}

	public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
	{
		foreach (string item in base.BlockingIssues(assignments, map))
		{
			yield return item;
		}
		Pawn pawn = assignments.FirstAssignedPawn(TargetRole);
		if (pawn != null && !pawn.skills.skills.Any((SkillRecord s) => !s.TotallyDisabled))
		{
			yield return "NoSkillsToTransfer".Translate(pawn.Named("TARGET"));
		}
	}

	public override IEnumerable<string> GetPawnTooltipExtras(Pawn pawn)
	{
		if (pawn.skills != null)
		{
			SkillRecord skillRecord = pawn.skills.skills.MaxBy((SkillRecord s) => s.XpTotalEarned);
			if (skillRecord.Level > 0)
			{
				yield return string.Concat("HighestSkill".Translate() + ": " + skillRecord.def.skillLabel.CapitalizeFirst() + " (" + "SkillLevel".Translate() + " ", skillRecord.Level.ToString(), ")");
			}
		}
	}
}
