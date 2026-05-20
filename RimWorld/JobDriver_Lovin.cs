using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Lovin : JobDriver
{
	private int ticksLeft;

	private TargetIndex PartnerInd = TargetIndex.A;

	private TargetIndex BedInd = TargetIndex.B;

	private const int TicksBetweenHeartMotes = 100;

	private static float PregnancyChance = 0.05f;

	private static readonly SimpleCurve LovinIntervalHoursFromAgeCurve = new SimpleCurve
	{
		new CurvePoint(16f, 1.5f),
		new CurvePoint(22f, 1.5f),
		new CurvePoint(30f, 4f),
		new CurvePoint(50f, 12f),
		new CurvePoint(75f, 36f)
	};

	private Pawn Partner => (Pawn)(Thing)job.GetTarget(PartnerInd);

	private Building_Bed Bed => (Building_Bed)(Thing)job.GetTarget(BedInd);

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(Partner, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed);
		}
		return false;
	}

	public override bool CanBeginNowWhileLyingDown()
	{
		return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(BedInd));
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(BedInd);
		this.FailOnDespawnedOrNull(PartnerInd);
		this.FailOn(() => !Partner.health.capacities.CanBeAwake);
		this.KeepLyingDown(BedInd);
		yield return Toils_Bed.ClaimBedIfNonMedical(BedInd);
		yield return Toils_Bed.GotoBed(BedInd);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (Partner.CurJob == null || Partner.CurJob.def != JobDefOf.Lovin)
			{
				Job newJob = JobMaker.MakeJob(JobDefOf.Lovin, pawn, Bed);
				Partner.jobs.StartJob(newJob, JobCondition.InterruptForced);
				ticksLeft = (int)(2500f * Mathf.Clamp(Rand.Range(0.1f, 1.1f), 0.1f, 2f));
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.InitiatedLovin, pawn.Named(HistoryEventArgsNames.Doer)));
				if (InteractionWorker_RomanceAttempt.CanCreatePsychicBondBetween(pawn, Partner) && InteractionWorker_RomanceAttempt.TryCreatePsychicBondBetween(pawn, Partner) && (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(Partner)))
				{
					Find.LetterStack.ReceiveLetter("LetterPsychicBondCreatedLovinLabel".Translate(), "LetterPsychicBondCreatedLovinText".Translate(pawn.Named("BONDPAWN"), Partner.Named("OTHERPAWN")), LetterDefOf.PositiveEvent, new LookTargets(pawn, Partner));
				}
			}
			else
			{
				ticksLeft = 9999999;
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
		Toil toil2 = Toils_LayDown.LayDown(BedInd, hasBed: true, lookForOtherJobs: false, canSleep: false, gainRestAndHealth: false);
		toil2.FailOn(() => Partner.CurJob == null || Partner.CurJob.def != JobDefOf.Lovin);
		toil2.AddPreTickIntervalAction(delegate(int delta)
		{
			ticksLeft -= delta;
			if (ticksLeft <= 0)
			{
				ReadyForNextToil();
			}
			else if (pawn.IsHashIntervalTick(100, delta))
			{
				FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Heart);
			}
		});
		toil2.AddFinishAction(delegate
		{
			Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.GotSomeLovin);
			if ((base.pawn.health != null && base.pawn.health.hediffSet != null && base.pawn.health.hediffSet.hediffs.Any((Hediff h) => h.def == HediffDefOf.LoveEnhancer)) || (Partner.health != null && Partner.health.hediffSet != null && Partner.health.hediffSet.hediffs.Any((Hediff h) => h.def == HediffDefOf.LoveEnhancer)))
			{
				thought_Memory.moodPowerFactor = 1.5f;
			}
			if (base.pawn.needs.mood != null)
			{
				base.pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, Partner);
			}
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.GotLovin, base.pawn.Named(HistoryEventArgsNames.Doer)));
			HistoryEventDef def = (base.pawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, Partner) ? HistoryEventDefOf.GotLovin_Spouse : HistoryEventDefOf.GotLovin_NonSpouse);
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(def, base.pawn.Named(HistoryEventArgsNames.Doer)));
			base.pawn.mindState.canLovinTick = Find.TickManager.TicksGame + GenerateRandomMinTicksToNextLovin(base.pawn);
			if (ModsConfig.BiotechActive)
			{
				Pawn pawn = ((base.pawn.gender == Gender.Male) ? base.pawn : ((Partner.gender == Gender.Male) ? Partner : null));
				Pawn pawn2 = ((base.pawn.gender == Gender.Female) ? base.pawn : ((Partner.gender == Gender.Female) ? Partner : null));
				if (pawn != null && pawn2 != null && Rand.Chance(PregnancyChance * PregnancyUtility.PregnancyChanceForPartners(pawn2, pawn)))
				{
					bool success;
					GeneSet inheritedGeneSet = PregnancyUtility.GetInheritedGeneSet(pawn, pawn2, out success);
					if (success)
					{
						Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, pawn2);
						hediff_Pregnant.SetParents(null, pawn, inheritedGeneSet);
						pawn2.health.AddHediff(hediff_Pregnant);
					}
					else if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(pawn2))
					{
						Messages.Message("MessagePregnancyFailed".Translate(pawn.Named("FATHER"), pawn2.Named("MOTHER")) + ": " + "CombinedGenesExceedMetabolismLimits".Translate(), new LookTargets(pawn, pawn2), MessageTypeDefOf.NegativeEvent);
					}
				}
			}
		});
		toil2.socialMode = RandomSocialMode.Off;
		yield return toil2;
	}

	private int GenerateRandomMinTicksToNextLovin(Pawn pawn)
	{
		if (DebugSettings.alwaysDoLovin)
		{
			return 100;
		}
		float num = LovinIntervalHoursFromAgeCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
		if (ModsConfig.BiotechActive && pawn.genes != null)
		{
			foreach (Gene item in pawn.genes.GenesListForReading)
			{
				num *= item.def.lovinMTBFactor;
			}
		}
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			HediffComp_GiveLovinMTBFactor hediffComp_GiveLovinMTBFactor = hediff.TryGetComp<HediffComp_GiveLovinMTBFactor>();
			if (hediffComp_GiveLovinMTBFactor != null)
			{
				num *= hediffComp_GiveLovinMTBFactor.Props.lovinMTBFactor;
			}
		}
		num = Rand.Gaussian(num, 0.3f);
		if (num < 0.5f)
		{
			num = 0.5f;
		}
		return (int)(num * 2500f);
	}
}
