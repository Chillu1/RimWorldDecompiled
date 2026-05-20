using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_LayDown : JobDriver
{
	private bool canMoveOrCrawl;

	private bool hasSavedValues;

	public const TargetIndex BedOrRestSpotIndex = TargetIndex.A;

	public Building_Bed Bed => job.GetTarget(TargetIndex.A).Thing as Building_Bed;

	public IntVec3 Cell => job.GetTarget(TargetIndex.A).Cell;

	public virtual bool CanSleep => true;

	public virtual bool CanRest => true;

	public virtual bool LookForOtherJobs => true;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (Bed != null)
		{
			if (!pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed))
			{
				return false;
			}
		}
		else if (job.targetA.Cell.IsValid && !pawn.ageTracker.CurLifeStage.alwaysDowned && !job.forceSleep && !pawn.Reserve(job.targetA.Cell, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		return true;
	}

	public override bool CanBeginNowWhileLyingDown()
	{
		return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		canMoveOrCrawl = !pawn.Downed || pawn.health.CanCrawl;
		hasSavedValues = true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		bool hasBed = Bed != null;
		if (hasBed)
		{
			yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
			yield return Toils_Bed.GotoBed(TargetIndex.A).FailOn(() => pawn.Downed && !pawn.health.CanCrawl && !Bed.OccupiedRect().Contains(pawn.Position));
		}
		else
		{
			Thing thing = job.GetTarget(TargetIndex.A).Thing;
			if ((thing == null || pawn.SpawnedParentOrMe != thing) && canMoveOrCrawl)
			{
				yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell).FailOn(() => pawn.Downed && !pawn.health.CanCrawl);
			}
		}
		yield return LayDownToil(hasBed);
	}

	public virtual Toil LayDownToil(bool hasBed)
	{
		return Toils_LayDown.LayDown(TargetIndex.A, hasBed, LookForOtherJobs, CanSleep, CanRest);
	}

	public override string GetReport()
	{
		string reportStringOverride = GetReportStringOverride();
		if (!reportStringOverride.NullOrEmpty())
		{
			return reportStringOverride;
		}
		if (asleep)
		{
			return "ReportSleeping".Translate();
		}
		Thing spawnedParentOrMe = pawn.SpawnedParentOrMe;
		if (spawnedParentOrMe != null && pawn != spawnedParentOrMe)
		{
			return JobDriver_Carried.GetReport(pawn, spawnedParentOrMe);
		}
		if (pawn.health.hediffSet.InLabor(includePostpartumExhaustion: false))
		{
			return "GivingBirth".Translate();
		}
		return "ReportResting".Translate();
	}

	public override bool IsContinuation(Job j)
	{
		if (job.GetTarget(TargetIndex.A) != j.GetTarget(TargetIndex.A))
		{
			return false;
		}
		return base.IsContinuation(j);
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref canMoveOrCrawl, "canMoveOrCrawl", defaultValue: false);
		Scribe_Values.Look(ref hasSavedValues, "hasSavedValues", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && !hasSavedValues)
		{
			canMoveOrCrawl = !pawn.Downed || pawn.health.CanCrawl;
			hasSavedValues = true;
		}
		base.ExposeData();
	}
}
