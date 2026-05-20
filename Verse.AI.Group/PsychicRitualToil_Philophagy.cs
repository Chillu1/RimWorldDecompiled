using System.Linq;
using RimWorld;

namespace Verse.AI.Group;

public class PsychicRitualToil_Philophagy : PsychicRitualToil
{
	public PsychicRitualRoleDef targetRole;

	public PsychicRitualRoleDef invokerRole;

	public FloatRange brainDamageRange;

	public float xpTransferPercent;

	protected PsychicRitualToil_Philophagy()
	{
	}

	public PsychicRitualToil_Philophagy(PsychicRitualRoleDef invokerRole, PsychicRitualRoleDef targetRole, FloatRange brainDamageRange)
	{
		this.invokerRole = invokerRole;
		this.targetRole = targetRole;
		this.brainDamageRange = brainDamageRange;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph graph)
	{
		PsychicRitualDef_Philophagy psychicRitualDef_Philophagy = (PsychicRitualDef_Philophagy)psychicRitual.def;
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		Pawn pawn2 = psychicRitual.assignments.FirstAssignedPawn(targetRole);
		xpTransferPercent = psychicRitualDef_Philophagy.xpTransferFromQualityCurve.Evaluate(psychicRitual.PowerPercent);
		if (pawn != null && pawn2 != null)
		{
			ApplyOutcome(psychicRitual, pawn, pawn2);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, Pawn target)
	{
		float xpTransfer;
		SkillDef philophagySkillAndXpTransfer = PsychicRitualUtility.GetPhilophagySkillAndXpTransfer(invoker, target, xpTransferPercent, out xpTransfer);
		if (philophagySkillAndXpTransfer == null)
		{
			Log.Error("Could not get skill for Philophagy ritual.");
			return;
		}
		int level = invoker.skills.GetSkill(philophagySkillAndXpTransfer).GetLevel();
		int level2 = target.skills.GetSkill(philophagySkillAndXpTransfer).GetLevel();
		invoker.skills.Learn(philophagySkillAndXpTransfer, xpTransfer, direct: true, ignoreLearnRate: true);
		target.skills.Learn(philophagySkillAndXpTransfer, 0f - xpTransfer, direct: true, ignoreLearnRate: true);
		target.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.PsychicRitualVictim);
		target.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.DrainedMySkills, invoker);
		foreach (Pawn item in psychicRitual.assignments.AllAssignedPawns.Except(target))
		{
			target.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.UsedMeForPsychicRitual, item);
		}
		target.health.AddHediff(HediffDefOf.DarkPsychicShock);
		BodyPartRecord brain = target.health.hediffSet.GetBrain();
		if (brain != null)
		{
			target.TakeDamage(new DamageInfo(DamageDefOf.Psychic, brainDamageRange.RandomInRange, 0f, -1f, null, brain));
		}
		int level3 = invoker.skills.GetSkill(philophagySkillAndXpTransfer).GetLevel();
		int level4 = target.skills.GetSkill(philophagySkillAndXpTransfer).GetLevel();
		if (target.Dead)
		{
			PsychicRitualUtility.RegisterAsExecutionIfPrisoner(target, invoker);
		}
		PsychicRitualUtility.AddPsychicRitualGuiltToPawns(psychicRitual.def, psychicRitual.Map.mapPawns.FreeColonistsSpawned.Where((Pawn p) => p != target));
		TaggedString text = "PhilophagyCompleteText".Translate(invoker.Named("INVOKER"), target.Named("TARGET"), psychicRitual.def.Named("RITUAL")) + string.Format("\n\n{0}", "PawnGainedXPInSkill".Translate(invoker, xpTransfer, philophagySkillAndXpTransfer)) + (", " + ((level3 > level) ? "PhilophagySkillLevelGained".Translate(level, level3).Resolve() : "PhilophagySkillLevelRemains".Translate(level3).Resolve())) + string.Format("\n\n{0}", "PawnLostXPInSkill".Translate(target, xpTransfer, philophagySkillAndXpTransfer)) + (", " + ((level4 < level2) ? "PhilophagySkillLevelLost".Translate(level2, level4).Resolve() : "PhilophagySkillLevelRemains".Translate(level4).Resolve())) + "\n\n" + (target.Dead ? "PsychicRitualTargetBrainLiquified" : "PhilophagyDamagedTarget").Translate(target.Named("TARGET"));
		Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), text, LetterDefOf.NeutralEvent, new LookTargets(invoker, target));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
		Scribe_Defs.Look(ref targetRole, "targetRole");
		Scribe_Values.Look(ref brainDamageRange, "brainDamageFromQualityRange");
		Scribe_Values.Look(ref xpTransferPercent, "xpTransferPercent", 0f);
	}
}
