using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Lovin : JobDriver
	{
		private int ticksLeft;

		private TargetIndex PartnerInd = TargetIndex.A;

		private TargetIndex BedInd = TargetIndex.B;

		private const int TicksBetweenHeartMotes = 100;

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
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (Partner.CurJob == null || Partner.CurJob.def != JobDefOf.Lovin)
				{
					Job newJob = JobMaker.MakeJob(JobDefOf.Lovin, pawn, Bed);
					Partner.jobs.StartJob(newJob, JobCondition.InterruptForced);
					ticksLeft = (int)(2500f * Mathf.Clamp(Rand.Range(0.1f, 1.1f), 0.1f, 2f));
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
			toil2.AddPreTickAction(delegate
			{
				ticksLeft--;
				if (ticksLeft <= 0)
				{
					ReadyForNextToil();
				}
				else if (pawn.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
				}
			});
			toil2.AddFinishAction(delegate
			{
				Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.GotSomeLovin);
				if ((pawn.health != null && pawn.health.hediffSet != null && pawn.health.hediffSet.hediffs.Any((Hediff h) => h.def == HediffDefOf.LoveEnhancer)) || (Partner.health != null && Partner.health.hediffSet != null && Partner.health.hediffSet.hediffs.Any((Hediff h) => h.def == HediffDefOf.LoveEnhancer)))
				{
					thought_Memory.moodPowerFactor = 1.5f;
				}
				if (pawn.needs.mood != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, Partner);
				}
				pawn.mindState.canLovinTick = Find.TickManager.TicksGame + GenerateRandomMinTicksToNextLovin(pawn);
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
			float centerX = LovinIntervalHoursFromAgeCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
			centerX = Rand.Gaussian(centerX, 0.3f);
			if (centerX < 0.5f)
			{
				centerX = 0.5f;
			}
			return (int)(centerX * 2500f);
		}
	}
}
