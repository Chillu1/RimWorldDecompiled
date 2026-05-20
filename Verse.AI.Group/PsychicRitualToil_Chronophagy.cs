using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public class PsychicRitualToil_Chronophagy : PsychicRitualToil
{
	public PsychicRitualRoleDef targetRole;

	public PsychicRitualRoleDef invokerRole;

	public FloatRange yearsTransferredFromQualityRange;

	protected PsychicRitualToil_Chronophagy()
	{
	}

	public PsychicRitualToil_Chronophagy(PsychicRitualRoleDef invokerRole, PsychicRitualRoleDef targetRole, FloatRange yearsTransferredFromQualityRange)
	{
		this.invokerRole = invokerRole;
		this.targetRole = targetRole;
		this.yearsTransferredFromQualityRange = yearsTransferredFromQualityRange;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		Pawn pawn2 = psychicRitual.assignments.FirstAssignedPawn(targetRole);
		float years = yearsTransferredFromQualityRange.LerpThroughRange(psychicRitual.PowerPercent);
		if (pawn != null && pawn2 != null)
		{
			ApplyOutcome(psychicRitual, pawn, pawn2, years);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, Pawn target, float years)
	{
		ReverseAgePawn(invoker, years, out var removedHediffs, out var pawnAgeDelta);
		AgePawn(psychicRitual, target, years, out var gainedHediffs, out var diedOfOldAge, out var diedOfBrainDamage);
		Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.DarkPsychicShock, target);
		target.health.AddHediff(hediff);
		target.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.PsychicRitualVictim);
		foreach (Pawn item in psychicRitual.assignments.AllAssignedPawns.Except(target))
		{
			target.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.UsedMeForPsychicRitual, item);
		}
		if (target.Dead)
		{
			PsychicRitualUtility.RegisterAsExecutionIfPrisoner(target, invoker);
		}
		PsychicRitualUtility.AddPsychicRitualGuiltToPawns(psychicRitual.def, psychicRitual.Map.mapPawns.FreeColonistsSpawned.Where((Pawn p) => p != target));
		GenerateLetter(psychicRitual, invoker, target, years, pawnAgeDelta, diedOfBrainDamage, diedOfOldAge, gainedHediffs, removedHediffs);
	}

	private static void ReverseAgePawn(Pawn pawn, float years, out List<Hediff> removedHediffs, out float pawnAgeDelta)
	{
		removedHediffs = new List<Hediff>();
		float num = Mathf.Max(pawn.ageTracker.AgeBiologicalYearsFloat - years, 13f);
		pawnAgeDelta = pawn.ageTracker.AgeBiologicalYearsFloat - num;
		pawn.ageTracker.AgeBiologicalTicks = Mathf.RoundToInt(num * 3600000f);
		pawn.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment);
		List<HediffGiverSetDef> hediffGiverSets = pawn.RaceProps.hediffGiverSets;
		if (hediffGiverSets == null)
		{
			return;
		}
		List<Hediff> resultHediffs = new List<Hediff>();
		foreach (HediffGiverSetDef item in hediffGiverSets)
		{
			List<HediffGiver> hediffGivers = item.hediffGivers;
			if (hediffGivers == null)
			{
				continue;
			}
			foreach (HediffGiver item2 in hediffGivers)
			{
				HediffGiver_Birthday agb = item2 as HediffGiver_Birthday;
				if (agb == null)
				{
					continue;
				}
				float num2 = num / pawn.RaceProps.lifeExpectancy;
				float x = agb.ageFractionChanceCurve.Points[0].x;
				if (!(num2 < x))
				{
					continue;
				}
				pawn.health.hediffSet.GetHediffs(ref resultHediffs, (Hediff hd) => hd.def == agb.hediff);
				foreach (Hediff item3 in resultHediffs)
				{
					pawn.health.RemoveHediff(item3);
					removedHediffs.Add(item3);
				}
			}
		}
		int num3 = Rand.RangeInclusive(1, 2);
		for (int num4 = pawn.health.hediffSet.hediffs.Count - 1; num4 >= 0; num4--)
		{
			Hediff hediff = pawn.health.hediffSet.hediffs[num4];
			if (hediff is Hediff_Injury && hediff.IsPermanent())
			{
				pawn.health.RemoveHediff(hediff);
				removedHediffs.Add(hediff);
				num3--;
				if (num3 <= 0)
				{
					break;
				}
			}
		}
	}

	private static void AgePawn(PsychicRitual psychicRitual, Pawn pawn, float years, out List<Hediff> gainedHediffs, out bool diedOfOldAge, out bool diedOfBrainDamage)
	{
		List<HediffGiver_Birthday> list = new List<HediffGiver_Birthday>();
		gainedHediffs = new List<Hediff>();
		diedOfBrainDamage = false;
		BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
		if (brain != null)
		{
			PsychicRitualDef_Chronophagy psychicRitualDef_Chronophagy = (PsychicRitualDef_Chronophagy)psychicRitual.def;
			pawn.TakeDamage(new DamageInfo(DamageDefOf.Psychic, psychicRitualDef_Chronophagy.brainDamageFromAgeCurve.Evaluate(pawn.ageTracker.AgeBiologicalYears), 0f, -1f, null, brain));
			diedOfBrainDamage = pawn.Dead;
		}
		for (int i = 1; (float)i <= years; i++)
		{
			list.AddRange(AgeInjuryUtility.RandomHediffsToGainOnBirthday(pawn, pawn.ageTracker.AgeBiologicalYears + i));
		}
		foreach (HediffGiver_Birthday item in list)
		{
			item.TryApply(pawn, gainedHediffs);
		}
		pawn.ageTracker.AgeBiologicalTicks += Mathf.RoundToInt(years * 3600000f);
		diedOfOldAge = !diedOfBrainDamage && pawn.Dead;
	}

	private static void GenerateLetter(PsychicRitual psychicRitual, Pawn invoker, Pawn target, float years, float invokerAgeDelta, bool diedOfBrainDamage, bool diedOfOldAge, List<Hediff> gainedHediffs, List<Hediff> curedHediffs)
	{
		TaggedString text = "ChronophagyCompleteText".Translate(invoker, psychicRitual.def.Named("RITUAL"));
		List<string> list = new List<string>();
		list.Add("ChronophagyInvokerRejuvenated".Translate(invoker, Mathf.FloorToInt(invokerAgeDelta)) + " " + "ChronophagyCurrentAge".Translate(invoker.ageTracker.AgeBiologicalYearsFloat));
		if (diedOfBrainDamage)
		{
			list.Add("PsychicRitualTargetBrainLiquified".Translate(target.Named("TARGET")));
		}
		else
		{
			list.Add("ChronophagyTargetAged".Translate(target, Mathf.FloorToInt(years)) + " " + "ChronophagyCurrentAge".Translate(target.ageTracker.AgeBiologicalYearsFloat));
		}
		text += "\n\n" + list.ToLineList("  - ");
		if (!diedOfBrainDamage)
		{
			if (diedOfOldAge)
			{
				text += "\n\n" + "ChronophagyTargetDiedOfOldAge".Translate(target);
			}
			else if (!gainedHediffs.NullOrEmpty())
			{
				text += "\n\n" + "ChronophagyTargetOldAgeDiseases".Translate(target) + ":";
				text += "\n\n" + gainedHediffs.Select((Hediff h) => h.LabelCap).ToLineList("  - ");
			}
		}
		if (!curedHediffs.NullOrEmpty())
		{
			text += "\n\n" + "ChronophagyInvokerCuredDiseases".Translate(invoker);
			text += "\n\n" + curedHediffs.Select((Hediff h) => h.LabelCap).ToLineList("  - ");
		}
		Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), text, LetterDefOf.NeutralEvent, new LookTargets(invoker, target));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
		Scribe_Defs.Look(ref targetRole, "targetRole");
		Scribe_Values.Look(ref yearsTransferredFromQualityRange, "yearsTransferredFromQualityRange");
	}
}
