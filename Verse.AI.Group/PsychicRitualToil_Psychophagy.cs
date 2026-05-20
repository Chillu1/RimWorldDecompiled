using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public class PsychicRitualToil_Psychophagy : PsychicRitualToil
{
	public PsychicRitualRoleDef targetRole;

	public PsychicRitualRoleDef invokerRole;

	public FloatRange brainDamageRange;

	public int effectDurationTicks;

	protected PsychicRitualToil_Psychophagy()
	{
	}

	public PsychicRitualToil_Psychophagy(PsychicRitualRoleDef invokerRole, PsychicRitualRoleDef targetRole, FloatRange brainDamageRange)
	{
		this.invokerRole = invokerRole;
		this.targetRole = targetRole;
		this.brainDamageRange = brainDamageRange;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		PsychicRitualDef_Psychophagy psychicRitualDef_Psychophagy = (PsychicRitualDef_Psychophagy)psychicRitual.def;
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		Pawn pawn2 = psychicRitual.assignments.FirstAssignedPawn(targetRole);
		effectDurationTicks = Mathf.RoundToInt(psychicRitualDef_Psychophagy.effectDurationDaysFromQualityCurve.Evaluate(psychicRitual.PowerPercent) * 60000f);
		if (pawn != null && pawn2 != null)
		{
			ApplyOutcome(psychicRitual, pawn, pawn2);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, Pawn target)
	{
		Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.DarkPsychicShock, target);
		target.health.AddHediff(hediff);
		Hediff hediff2 = HediffMaker.MakeHediff(HediffDefOf.PsychicallyDead, target);
		target.health.AddHediff(hediff2);
		target.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.PsychicRitualVictim);
		foreach (Pawn item in psychicRitual.assignments.AllAssignedPawns.Except(target))
		{
			target.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.UsedMeForPsychicRitual, item);
		}
		BodyPartRecord brain = target.health.hediffSet.GetBrain();
		if (brain != null)
		{
			target.TakeDamage(new DamageInfo(DamageDefOf.Psychic, brainDamageRange.RandomInRange, 0f, -1f, null, brain));
		}
		Hediff firstHediffOfDef = invoker.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Psychophage);
		if (firstHediffOfDef == null)
		{
			firstHediffOfDef = HediffMaker.MakeHediff(HediffDefOf.Psychophage, invoker);
			HediffComp_Disappears hediffComp_Disappears = firstHediffOfDef.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears != null)
			{
				hediffComp_Disappears.ticksToDisappear = effectDurationTicks;
			}
			invoker.health.AddHediff(firstHediffOfDef);
		}
		else
		{
			HediffComp_Disappears hediffComp_Disappears2 = firstHediffOfDef.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears2 != null)
			{
				hediffComp_Disappears2.ticksToDisappear += effectDurationTicks;
			}
		}
		if (target.Dead)
		{
			PsychicRitualUtility.RegisterAsExecutionIfPrisoner(target, invoker);
		}
		PsychicRitualUtility.AddPsychicRitualGuiltToPawns(psychicRitual.def, psychicRitual.Map.mapPawns.FreeColonistsSpawned.Where((Pawn p) => p != target));
		TaggedString text = "PsychophagyCompleteText".Translate(invoker.Named("INVOKER"), psychicRitual.def.Named("RITUAL"), target.Named("TARGET"));
		if (target.Dead)
		{
			text += "\n\n" + "PsychicRitualTargetBrainLiquified".Translate(target.Named("TARGET"));
		}
		else
		{
			text += "\n\n" + "PsychophagyTargetSurvived".Translate(target.Named("TARGET"));
		}
		Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), text, LetterDefOf.NeutralEvent, new LookTargets(invoker, target));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
		Scribe_Defs.Look(ref targetRole, "targetRole");
		Scribe_Values.Look(ref brainDamageRange, "brainDamageRange");
		Scribe_Values.Look(ref effectDurationTicks, "effectDurationTicks", 0);
	}
}
