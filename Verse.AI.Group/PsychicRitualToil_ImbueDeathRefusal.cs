using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public class PsychicRitualToil_ImbueDeathRefusal : PsychicRitualToil
{
	public PsychicRitualRoleDef targetRole;

	private static List<Pawn> tmpTargetPawns = new List<Pawn>(4);

	protected PsychicRitualToil_ImbueDeathRefusal()
	{
	}

	public PsychicRitualToil_ImbueDeathRefusal(PsychicRitualRoleDef targetRole)
	{
		this.targetRole = targetRole;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph graph)
	{
		float skillOffsetPct = ((PsychicRitualDef_ImbueDeathRefusal)psychicRitual.def).skillOffsetPercentFromQualityCurve.Evaluate(psychicRitual.PowerPercent);
		tmpTargetPawns.Clear();
		tmpTargetPawns.AddRange(psychicRitual.assignments.AssignedPawns(targetRole));
		foreach (Pawn tmpTargetPawn in tmpTargetPawns)
		{
			ApplyOutcome(psychicRitual, tmpTargetPawn, skillOffsetPct);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn pawn, float skillOffsetPct)
	{
		Hediff_DeathRefusal firstHediff = pawn.health.hediffSet.GetFirstHediff<Hediff_DeathRefusal>();
		if (firstHediff != null)
		{
			firstHediff.SetUseAmountDirect(firstHediff.UsesLeft + 1);
		}
		else
		{
			Hediff_DeathRefusal hediff_DeathRefusal = (Hediff_DeathRefusal)HediffMaker.MakeHediff(HediffDefOf.DeathRefusal, pawn);
			hediff_DeathRefusal.SetUseAmountDirect(1);
			pawn.health.AddHediff(hediff_DeathRefusal);
		}
		float num = 0f;
		foreach (SkillRecord skill in pawn.skills.skills)
		{
			float num2 = skill.XpTotalEarned * skillOffsetPct;
			skill.Learn(num2);
			num += num2;
		}
		if (PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), "ImbueDeathRefuralCompleteText".Translate(pawn, Mathf.Abs((int)num)), LetterDefOf.NeutralEvent, pawn);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref targetRole, "targetRole");
	}
}
