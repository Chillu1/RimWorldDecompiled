using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;

namespace Verse.AI;

public class Pawn_JobTracker : IExposable
{
	protected Pawn pawn;

	public Job curJob;

	public JobDriver curDriver;

	public JobQueue jobQueue = new JobQueue();

	public PawnPosture posture;

	private int startedFormingCaravanTickInt = -1;

	public bool startingNewJob;

	private bool determiningNextJob;

	private int jobsGivenThisTick;

	private string jobsGivenThisTickTextual = "";

	private int lastJobGivenAtFrame = -1;

	private int lastTickFinalized = -1;

	private List<int> jobsGivenRecentTicks = new List<int>(10);

	private List<string> jobsGivenRecentTicksTextual = new List<string>(10);

	public bool debugLog;

	private const int RecentJobQueueMaxLength = 10;

	private const int MaxRecentJobs = 10;

	private const int NearbyEnemyMaxRegions = 25;

	private static readonly SimpleCurve DistanceIntervalCurve = new SimpleCurve
	{
		new CurvePoint(0f, 30f),
		new CurvePoint(50f, 120f),
		new CurvePoint(100f, 600f)
	};

	private int lastDamageCheckTick = -99999;

	private const int DamageCheckMinInterval = 180;

	public bool HandlingFacing
	{
		get
		{
			if (curDriver != null)
			{
				return curDriver.HandlingFacing;
			}
			return false;
		}
	}

	public bool DeterminingNextJob => determiningNextJob;

	public int StartedFormingCaravanTick
	{
		get
		{
			if (!pawn.IsFormingCaravan())
			{
				startedFormingCaravanTickInt = -1;
			}
			return startedFormingCaravanTickInt;
		}
	}

	public Pawn_JobTracker(Pawn newPawn)
	{
		pawn = newPawn;
	}

	public virtual void ExposeData()
	{
		Scribe_Deep.Look(ref curJob, "curJob");
		Scribe_Deep.Look(ref curDriver, "curDriver");
		Scribe_Deep.Look(ref jobQueue, "jobQueue");
		Scribe_Values.Look(ref posture, "posture", PawnPosture.Standing);
		Scribe_Values.Look(ref startedFormingCaravanTickInt, "formingCaravanTick", 0);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (curDriver != null)
			{
				curDriver.pawn = pawn;
				curDriver.job = curJob;
			}
		}
		else if (Scribe.mode == LoadSaveMode.PostLoadInit && curDriver == null && curJob != null)
		{
			Log.Warning($"Cleaning up invalid job state on {pawn}");
			EndCurrentJob(JobCondition.Errored);
		}
	}

	public void Notify_WorkTypeDisabled(WorkTypeDef wType)
	{
		bool flag = pawn.WorkTypeIsDisabled(wType);
		jobQueue.RemoveAllWorkType(pawn, wType, flag);
		if (curJob != null && curJob.workGiverDef != null && curJob.workGiverDef.workType == wType && (flag || !curJob.playerForced))
		{
			EndCurrentJob(JobCondition.InterruptForced);
		}
	}

	public void Notify_JoyKindDisabled(JoyKindDef joyKind)
	{
		jobQueue.RemoveAllJoyKind(pawn, joyKind);
		if (curJob != null && curJob.def.joyKind == joyKind)
		{
			EndCurrentJob(JobCondition.InterruptForced);
		}
	}

	public virtual void JobTrackerTick()
	{
		curDriver?.DriverTick();
		jobsGivenThisTick = 0;
		jobsGivenThisTickTextual = "";
	}

	public virtual void JobTrackerTickInterval(int delta)
	{
		if (pawn.IsHashIntervalTick(30, delta))
		{
			ThinkResult thinkResult = DetermineNextConstantThinkTreeJob();
			if (thinkResult.IsValid)
			{
				if (ShouldStartJobFromThinkTree(thinkResult))
				{
					CheckLeaveJoinableLordBecauseJobIssued(thinkResult);
					StartJob(thinkResult.Job, JobCondition.InterruptForced, thinkResult.SourceNode, resumeCurJobAfterwards: false, cancelBusyStances: false, pawn.thinker.ConstantThinkTree, thinkResult.Tag);
				}
				else if (thinkResult.Job != curJob && !jobQueue.Contains(thinkResult.Job))
				{
					JobMaker.ReturnToPool(thinkResult.Job);
				}
			}
		}
		if (curDriver != null)
		{
			if (GenTicks.TicksGame != curJob.startTick)
			{
				bool flag = false;
				if (curJob.intervalScalingTarget != TargetIndex.None)
				{
					int lengthManhattan = (curJob.GetTarget(curJob.intervalScalingTarget).Cell - pawn.Position).LengthManhattan;
					curJob.expiryInterval = (int)DistanceIntervalCurve.Evaluate(lengthManhattan);
				}
				if (curJob.expiryInterval > 0)
				{
					flag = curJob.startTick + curJob.expiryInterval <= GenTicks.TicksGame && pawn.IsHashIntervalTick(curJob.expiryInterval, delta);
				}
				if (flag)
				{
					if (!curJob.expireRequiresEnemiesNearby || PawnUtility.EnemiesAreNearby(pawn, 25))
					{
						if (debugLog)
						{
							DebugLogEvent("Job expire");
						}
						if (!curJob.checkOverrideOnExpire)
						{
							EndCurrentJob(JobCondition.Succeeded);
						}
						else
						{
							CheckForJobOverride();
						}
						FinalizeTick();
						return;
					}
					if (debugLog)
					{
						DebugLogEvent("Job expire skipped because there are no enemies nearby");
					}
				}
			}
			curDriver.DriverTickInterval(delta);
		}
		if (curJob == null && !pawn.Dead && pawn.mindState.Active)
		{
			if (debugLog)
			{
				DebugLogEvent("Starting job from Tick because curJob == null.");
			}
			TryFindAndStartJob();
		}
		FinalizeTick();
	}

	private void FinalizeTick()
	{
		jobsGivenRecentTicks.Add(jobsGivenThisTick);
		jobsGivenRecentTicksTextual.Add(jobsGivenThisTickTextual);
		int num = GenTicks.TicksGame - lastTickFinalized;
		lastTickFinalized = GenTicks.TicksGame;
		for (int i = 0; i < num; i++)
		{
			if (jobsGivenRecentTicks.Count <= 0)
			{
				break;
			}
			jobsGivenRecentTicks.RemoveAt(0);
			jobsGivenRecentTicksTextual.RemoveAt(0);
		}
		if (jobsGivenThisTick != 0)
		{
			int num2 = 0;
			for (int j = 0; j < jobsGivenRecentTicks.Count; j++)
			{
				num2 += jobsGivenRecentTicks[j];
			}
			if (num2 >= 10)
			{
				string text = jobsGivenRecentTicksTextual.ToCommaList();
				jobsGivenRecentTicks.Clear();
				jobsGivenRecentTicksTextual.Clear();
				JobUtility.TryStartErrorRecoverJob(pawn, $"{pawn.ToStringSafe()} started {10} jobs in {10} ticks. List: {text}");
			}
		}
	}

	public IEnumerable<Job> AllJobs()
	{
		if (curJob != null)
		{
			yield return curJob;
		}
		foreach (QueuedJob item in jobQueue)
		{
			yield return item.job;
		}
	}

	public void StartJob(Job newJob, JobCondition lastJobEndCondition = JobCondition.None, ThinkNode jobGiver = null, bool resumeCurJobAfterwards = false, bool cancelBusyStances = true, ThinkTreeDef thinkTree = null, JobTag? tag = null, bool fromQueue = false, bool canReturnCurJobToPool = false, bool? keepCarryingThingOverride = null, bool continueSleeping = false, bool addToJobsThisTick = true, bool preToilReservationsCanFail = false)
	{
		startingNewJob = true;
		Job job = null;
		try
		{
			if (addToJobsThisTick && !fromQueue && (!Find.TickManager.Paused || lastJobGivenAtFrame == RealTime.frameCount))
			{
				jobsGivenThisTick++;
				if (Prefs.DevMode)
				{
					jobsGivenThisTickTextual = jobsGivenThisTickTextual + "(" + newJob?.ToString() + ") ";
				}
			}
			lastJobGivenAtFrame = RealTime.frameCount;
			if (jobsGivenThisTick > 10)
			{
				string text = jobsGivenThisTickTextual;
				jobsGivenThisTick = 0;
				jobsGivenThisTickTextual = "";
				startingNewJob = false;
				pawn.ClearReservationsForJob(newJob);
				JobUtility.TryStartErrorRecoverJob(pawn, pawn.ToStringSafe() + " started 10 jobs in one tick. newJob=" + newJob.ToStringSafe() + " jobGiver=" + jobGiver.ToStringSafe() + " jobList=" + text);
				return;
			}
			if (debugLog)
			{
				DebugLogEvent($"StartJob [{newJob}] lastJobEndCondition={lastJobEndCondition}, jobGiver={jobGiver}, cancelBusyStances={cancelBusyStances}");
			}
			if (cancelBusyStances && pawn.stances.FullBodyBusy)
			{
				pawn.stances.CancelBusyStanceHard();
			}
			bool asleep = continueSleeping && (curDriver?.asleep ?? false);
			if (curJob != null)
			{
				if (lastJobEndCondition == JobCondition.None)
				{
					Log.Warning(pawn?.ToString() + " starting job " + newJob?.ToString() + " from JobGiver " + newJob.jobGiver?.ToString() + " while already having job " + curJob?.ToString() + " without a specific job end condition.");
					lastJobEndCondition = JobCondition.InterruptForced;
				}
				if (resumeCurJobAfterwards && curJob.def.suspendable)
				{
					SuspendCurrentJob(lastJobEndCondition, cancelBusyStances, keepCarryingThingOverride);
				}
				else
				{
					job = curDriver.GetFinalizerJob(lastJobEndCondition);
					if (job != null)
					{
						bool valueOrDefault = keepCarryingThingOverride == true;
						if (!keepCarryingThingOverride.HasValue)
						{
							valueOrDefault = true;
							keepCarryingThingOverride = valueOrDefault;
						}
					}
					CleanupCurrentJob(lastJobEndCondition, releaseReservations: true, cancelBusyStances, canReturnCurJobToPool, keepCarryingThingOverride);
				}
			}
			if (newJob == null)
			{
				Log.Warning(pawn?.ToString() + " tried to start doing a null job.");
				return;
			}
			newJob.startTick = Find.TickManager.TicksGame;
			if (pawn.Drafted || newJob.playerForced)
			{
				newJob.ignoreForbidden = true;
				newJob.ignoreDesignations = true;
			}
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				if (hediff.def.TryGetReportStringOverrideFor(newJob.def, out var str))
				{
					newJob.reportStringOverride = str;
					break;
				}
			}
			curJob = newJob;
			curJob.jobGiverThinkTree = thinkTree;
			curJob.jobGiver = jobGiver;
			curDriver = curJob.MakeDriver(pawn);
			curDriver.asleep = asleep;
			if (curDriver.TryMakePreToilReservations(!preToilReservationsCanFail && !fromQueue))
			{
				Job job2 = TryOpportunisticJob(job, newJob);
				if (job2 != null)
				{
					jobQueue.EnqueueFirst(newJob);
					curJob = null;
					ClearDriver();
					bool? keepCarryingThingOverride2 = keepCarryingThingOverride;
					StartJob(job2, JobCondition.None, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, keepCarryingThingOverride2);
					return;
				}
				if (tag.HasValue)
				{
					if (tag == JobTag.Fieldwork && pawn.mindState.lastJobTag != tag)
					{
						foreach (Pawn item in PawnUtility.SpawnedMasteredPawns(pawn))
						{
							item.jobs.Notify_MasterStartedFieldWork();
						}
					}
					pawn.mindState.lastJobTag = tag.Value;
				}
				if (pawn.IsCarrying() && !(keepCarryingThingOverride ?? (!curJob.def.dropThingBeforeJob)))
				{
					if (DebugViewSettings.logCarriedBetweenJobs)
					{
						Log.Message($"Dropping {pawn.carryTracker.CarriedThing} before starting job {newJob}");
					}
					pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
				}
				curDriver.SetInitialPosture();
				curDriver.Notify_Starting();
				curDriver.SetupToils();
				curDriver.ReadyForNextToil();
				pawn.flight?.Notify_JobStarted(newJob);
			}
			else if (preToilReservationsCanFail || fromQueue)
			{
				EndCurrentJob(JobCondition.QueuedNoLongerValid);
			}
			else
			{
				Log.Warning($"TryMakePreToilReservations() returned false for a non-queued job right after StartJob(). This should have been checked before. pawn = {pawn}, curJob = {curJob.ToStringSafe()}");
				EndCurrentJob(JobCondition.Errored);
			}
		}
		finally
		{
			startingNewJob = false;
		}
	}

	public void EndCurrentOrQueuedJob(Job job, JobCondition condition, bool startNewJob = true, bool canReturnToPool = true)
	{
		if (debugLog)
		{
			DebugLogEvent($"EndJob [{job}] condition={condition}");
		}
		jobQueue.Extract(job)?.Cleanup(pawn, canReturnToPool);
		if (curJob == job)
		{
			EndCurrentJob(condition, startNewJob, canReturnToPool);
		}
	}

	public void SuspendCurrentJob(JobCondition jobPauseReason, bool cancelBusyStances = true, bool? carryThingAfterJobOverride = null)
	{
		if (debugLog)
		{
			DebugLogEvent("SuspendCurrentJob " + ((curJob != null) ? curJob.ToString() : "null") + " curToil=" + CurToilString(curDriver));
		}
		if (curJob.def.suspendable)
		{
			jobQueue.EnqueueFirst(curJob);
			CleanupCurrentJob(jobPauseReason, releaseReservations: false, cancelBusyStances, canReturnToPool: false, carryThingAfterJobOverride);
		}
		else
		{
			CleanupCurrentJob(jobPauseReason, releaseReservations: true, cancelBusyStances, canReturnToPool: false, carryThingAfterJobOverride);
		}
	}

	public void EndCurrentJob(JobCondition condition, bool startNewJob = true, bool canReturnToPool = true)
	{
		if (debugLog)
		{
			DebugLogEvent("EndCurrentJob " + ((curJob != null) ? curJob.ToString() : "null") + " condition=" + condition.ToString() + " curToil=" + CurToilString(curDriver));
		}
		if (condition == JobCondition.Ongoing)
		{
			Log.Warning("Ending a job with Ongoing as the condition. This makes no sense.");
		}
		JobDef jobDef = curJob?.def;
		bool collideWithPawns = false;
		if (curJob != null)
		{
			collideWithPawns = curJob.collideWithPawns || curJob.def.collideWithPawns;
		}
		Job job = curDriver?.GetFinalizerJob(condition);
		if (job != null && curJob != null)
		{
			job.playerForced = curJob.playerForced;
		}
		bool? carryThingAfterJobOverride = null;
		if (startNewJob && job != null)
		{
			carryThingAfterJobOverride = true;
		}
		CleanupCurrentJob(condition, releaseReservations: true, cancelBusyStancesSoft: true, canReturnToPool, carryThingAfterJobOverride);
		if (!startNewJob)
		{
			return;
		}
		if (condition == JobCondition.ErroredPather || condition == JobCondition.Errored)
		{
			StartJob(JobMaker.MakeJob(JobDefOf.Wait, 250));
			return;
		}
		if (job != null && CanPawnTakeOpportunisticJob(pawn))
		{
			bool? keepCarryingThingOverride = true;
			StartJob(job, JobCondition.None, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, keepCarryingThingOverride);
			return;
		}
		if (condition == JobCondition.Succeeded && jobDef != null && jobDef != JobDefOf.Wait_MaintainPosture && jobDef != JobDefOf.Goto)
		{
			Pawn_PathFollower pather = pawn.pather;
			if (pather != null && !pather.Moving)
			{
				Job job2 = JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture, 1);
				job2.collideWithPawns = collideWithPawns;
				StartJob(job2, JobCondition.None, null, resumeCurJobAfterwards: false, cancelBusyStances: false, null, null, fromQueue: false, canReturnCurJobToPool: false, null, continueSleeping: false, addToJobsThisTick: false);
				return;
			}
		}
		TryFindAndStartJob();
	}

	private void CleanupCurrentJob(JobCondition condition, bool releaseReservations, bool cancelBusyStancesSoft = true, bool canReturnToPool = false, bool? carryThingAfterJobOverride = null)
	{
		if (debugLog)
		{
			DebugLogEvent(string.Format("CleanupCurrentJob {0} condition {1}", (curJob != null) ? curJob.def.ToString() : "null", condition));
		}
		if (curJob == null)
		{
			return;
		}
		pawn.GetLord()?.Notify_PawnJobDone(pawn, condition);
		if (condition == JobCondition.Succeeded && curJob.def.taleOnCompletion != null)
		{
			TaleRecorder.RecordTale(curJob.def.taleOnCompletion, curDriver.TaleParameters());
		}
		if (releaseReservations)
		{
			pawn.ClearReservationsForJob(curJob);
		}
		if (curDriver != null)
		{
			curDriver.ended = true;
			curDriver.Cleanup(condition);
		}
		ClearDriver();
		Job job = curJob;
		curJob = null;
		if (!releaseReservations)
		{
			pawn.VerifyReservations(job);
		}
		if (cancelBusyStancesSoft)
		{
			pawn.stances.CancelBusyStanceSoft();
		}
		if (pawn.IsCarrying() && !(carryThingAfterJobOverride ?? job.def.carryThingAfterJob))
		{
			if (DebugViewSettings.logCarriedBetweenJobs)
			{
				Log.Message($"Dropping {pawn.carryTracker.CarriedThing} after finishing job {job}");
			}
			pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
		}
		if (releaseReservations && canReturnToPool)
		{
			JobMaker.ReturnToPool(job);
		}
	}

	public JobQueue CaptureAndClearJobQueue()
	{
		JobQueue result = jobQueue.Capture();
		ClearQueuedJobs(canReturnToPool: false);
		return result;
	}

	public void RestoreCapturedJobs(JobQueue incomming, bool canReturnToPool = true)
	{
		bool flag = false;
		QueuedJob queuedJob;
		while ((queuedJob = incomming.Dequeue()) != null)
		{
			if (flag)
			{
				if (canReturnToPool)
				{
					JobMaker.ReturnToPool(queuedJob.job);
				}
			}
			else if (!pawn.jobs.TryTakeOrderedJob(queuedJob.job, queuedJob.tag, requestQueueing: true))
			{
				flag = true;
			}
		}
	}

	public void ClearQueuedJobs(bool canReturnToPool = true)
	{
		if (debugLog)
		{
			DebugLogEvent("ClearQueuedJobs");
		}
		jobQueue.Clear(pawn, canReturnToPool);
	}

	public void ReleaseReservations(LocalTargetInfo reservedItem)
	{
		foreach (QueuedJob item in jobQueue)
		{
			if (pawn.MapHeld.reservationManager.ReservedBy(reservedItem, pawn, item.job))
			{
				pawn.MapHeld.reservationManager.Release(reservedItem, pawn, item.job);
			}
		}
	}

	public void CheckForJobOverride(float minPriority = 0f, bool ignoreQueue = true)
	{
		if (debugLog)
		{
			DebugLogEvent("CheckForJobOverride");
		}
		ThinkTreeDef thinkTree;
		ThinkResult thinkResult = DetermineNextJob(out thinkTree, ignoreQueue);
		if (thinkResult.IsValid)
		{
			if (ShouldStartJobFromThinkTree(thinkResult) && (minPriority == 0f || (thinkResult.SourceNode.TryGetPriority(pawn, out var value) && value >= minPriority)))
			{
				CheckLeaveJoinableLordBecauseJobIssued(thinkResult);
				StartJob(thinkResult.Job, JobCondition.InterruptOptional, thinkResult.SourceNode, resumeCurJobAfterwards: false, cancelBusyStances: false, thinkTree, thinkResult.Tag, thinkResult.FromQueue);
			}
			else if (thinkResult.Job != curJob && !jobQueue.Contains(thinkResult.Job))
			{
				JobMaker.ReturnToPool(thinkResult.Job);
			}
		}
	}

	public void StopAll(bool ifLayingKeepLaying = false, bool canReturnToPool = true)
	{
		if (!RestUtility.IsLayingForJobCleanup(pawn) || !ifLayingKeepLaying)
		{
			CleanupCurrentJob(JobCondition.InterruptForced, releaseReservations: true, cancelBusyStancesSoft: true, canReturnToPool);
		}
		ClearQueuedJobs(canReturnToPool);
	}

	private void TryFindAndStartJob()
	{
		if (pawn.thinker == null)
		{
			Log.ErrorOnce(pawn?.ToString() + " did TryFindAndStartJob but had no thinker.", 8573261);
			return;
		}
		if (curJob != null)
		{
			Log.Warning(pawn?.ToString() + " doing TryFindAndStartJob while still having job " + curJob);
		}
		if (debugLog)
		{
			DebugLogEvent("TryFindAndStartJob");
		}
		ThinkTreeDef thinkTree;
		ThinkResult result = DetermineNextJob(out thinkTree);
		if (result.IsValid)
		{
			CheckLeaveJoinableLordBecauseJobIssued(result);
			StartJob(result.Job, JobCondition.None, result.SourceNode, resumeCurJobAfterwards: false, cancelBusyStances: false, thinkTree, result.Tag, result.FromQueue);
		}
	}

	private static bool CanPawnTakeOpportunisticJob(Pawn pawn)
	{
		if (pawn.Drafted)
		{
			return false;
		}
		if (pawn.Downed)
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && pawn.IsSubhuman)
		{
			return false;
		}
		if (pawn.InMentalState || pawn.IsBurning())
		{
			return false;
		}
		if (SlaveRebellionUtility.IsRebelling(pawn))
		{
			return false;
		}
		return true;
	}

	public Job TryOpportunisticJob(Job finalizerJob, Job job)
	{
		if (!CanPawnTakeOpportunisticJob(pawn))
		{
			return null;
		}
		if (!job.def.allowOpportunisticPrefix)
		{
			return null;
		}
		if (finalizerJob != null)
		{
			return finalizerJob;
		}
		IntVec3 cell = job.targetA.Cell;
		if (!cell.IsValid || cell.IsForbidden(pawn))
		{
			return null;
		}
		if ((int)pawn.RaceProps.intelligence < 2)
		{
			return null;
		}
		if (pawn.Faction != Faction.OfPlayer)
		{
			return null;
		}
		if (job.playerForced)
		{
			return null;
		}
		if (pawn.WorkTagIsDisabled(WorkTags.ManualDumb | WorkTags.Hauling | WorkTags.AllWork))
		{
			return null;
		}
		if (ModsConfig.BiotechActive && pawn.IsWorkTypeDisabledByAge(WorkTypeDefOf.Hauling, out var _))
		{
			return null;
		}
		float num = pawn.Position.DistanceTo(cell);
		if (num < 3f)
		{
			return null;
		}
		if (!pawn.Spawned)
		{
			return null;
		}
		foreach (Thing item in pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling())
		{
			float num2 = pawn.Position.DistanceTo(item.Position);
			if (num2 > 30f || num2 > num * 0.5f || num2 + item.Position.DistanceTo(cell) > num * 1.7f || pawn.Map.reservationManager.FirstRespectedReserver(item, pawn) != null || item.IsForbidden(pawn) || !HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, item, forced: false))
			{
				continue;
			}
			StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(item, job.playerForced);
			if (!StoreUtility.TryFindBestBetterStorageFor(item, pawn, pawn.Map, currentPriority, pawn.Faction, out var foundCell, out var haulDestination))
			{
				continue;
			}
			IntVec3 intVec;
			if (haulDestination is ISlotGroupParent)
			{
				intVec = foundCell;
			}
			else
			{
				if (!(haulDestination is Thing thing) || thing.TryGetInnerInteractableThingOwner() == null)
				{
					Log.Error("Don't know how to handle opportunistic hauling for Storage: " + haulDestination.ToStringSafe() + ", Thing: " + item.ToStringSafe());
					continue;
				}
				intVec = thing.Position;
			}
			float num3 = intVec.DistanceTo(cell);
			if (!(num3 > 50f) && !(num3 > num * 0.6f) && !(num2 + item.Position.DistanceTo(intVec) + num3 > num * 1.7f) && !(num2 + num3 > num) && pawn.Position.WithinRegions(item.Position, pawn.Map, 25, TraverseParms.For(pawn)) && intVec.WithinRegions(cell, pawn.Map, 25, TraverseParms.For(pawn)))
			{
				if (DebugViewSettings.drawOpportunisticJobs)
				{
					Log.Message("Opportunistic job spawned");
					pawn.Map.debugDrawer.FlashLine(pawn.Position, item.Position, 600, SimpleColor.Red);
					pawn.Map.debugDrawer.FlashLine(item.Position, intVec, 600, SimpleColor.Green);
					pawn.Map.debugDrawer.FlashLine(intVec, cell, 600, SimpleColor.Blue);
				}
				if (haulDestination is ISlotGroupParent)
				{
					return HaulAIUtility.HaulToCellStorageJob(pawn, item, foundCell, fitInStoreCell: false);
				}
				if (haulDestination is Thing container)
				{
					return HaulAIUtility.HaulToContainerJob(pawn, item, container);
				}
			}
		}
		return null;
	}

	private ThinkResult DetermineNextJob(out ThinkTreeDef thinkTree, bool ignoreQueue = false)
	{
		if (determiningNextJob)
		{
			thinkTree = null;
			return ThinkResult.NoJob;
		}
		determiningNextJob = true;
		ThinkResult result = DetermineNextConstantThinkTreeJob();
		if (result.Job != null)
		{
			thinkTree = pawn.thinker.ConstantThinkTree;
			determiningNextJob = false;
			return result;
		}
		ThinkResult result2;
		try
		{
			result2 = pawn.thinker.MainThinkNodeRoot.TryIssueJobPackage(pawn, new JobIssueParams
			{
				ignoreQueue = ignoreQueue
			});
		}
		catch (Exception exception)
		{
			JobUtility.TryStartErrorRecoverJob(pawn, pawn.ToStringSafe() + " threw exception while determining job (main)", exception);
			thinkTree = null;
			determiningNextJob = false;
			return ThinkResult.NoJob;
		}
		finally
		{
		}
		thinkTree = pawn.thinker.MainThinkTree;
		determiningNextJob = false;
		return result2;
	}

	private ThinkResult DetermineNextConstantThinkTreeJob()
	{
		if (pawn.thinker?.ConstantThinkTree == null)
		{
			return ThinkResult.NoJob;
		}
		try
		{
			return pawn.thinker.ConstantThinkNodeRoot.TryIssueJobPackage(pawn, default(JobIssueParams));
		}
		catch (Exception exception)
		{
			JobUtility.TryStartErrorRecoverJob(pawn, pawn.ToStringSafe() + " threw exception while determining job (constant)", exception);
		}
		finally
		{
		}
		return ThinkResult.NoJob;
	}

	private void CheckLeaveJoinableLordBecauseJobIssued(ThinkResult result)
	{
		if (!result.IsValid || result.SourceNode == null)
		{
			return;
		}
		Lord lord = pawn.GetLord();
		if (lord == null || !(lord.LordJob is LordJob_VoluntarilyJoinable))
		{
			return;
		}
		bool flag = false;
		ThinkNode thinkNode = result.SourceNode;
		do
		{
			if (thinkNode.leaveJoinableLordIfIssuesJob)
			{
				flag = true;
				break;
			}
			thinkNode = thinkNode.parent;
		}
		while (thinkNode != null);
		if (flag)
		{
			lord.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily);
		}
	}

	private bool ShouldStartJobFromThinkTree(ThinkResult thinkResult)
	{
		if (curJob == null)
		{
			return true;
		}
		if (curJob == thinkResult.Job)
		{
			return false;
		}
		if (!thinkResult.FromQueue && thinkResult.Job.def == curJob.def && curDriver.IsContinuation(thinkResult.Job))
		{
			return thinkResult.SourceNode != curJob.jobGiver;
		}
		return true;
	}

	public void SetFormingCaravanTick(bool clear = false)
	{
		startedFormingCaravanTickInt = (clear ? (-1) : Find.TickManager.TicksGame);
	}

	public bool IsCurrentJobPlayerInterruptible()
	{
		if (curJob != null)
		{
			if (!curJob.def.playerInterruptible)
			{
				return false;
			}
			if (curDriver != null && !curDriver.PlayerInterruptable)
			{
				return false;
			}
		}
		if (pawn.HasAttachment(ThingDefOf.Fire))
		{
			return false;
		}
		return true;
	}

	public bool TryTakeOrderedJobPrioritizedWork(Job job, WorkGiver giver, IntVec3 cell)
	{
		if (TryTakeOrderedJob(job, giver.def.tagToGive))
		{
			job.workGiverDef = giver.def;
			if (giver.def.prioritizeSustains)
			{
				pawn.mindState.priorityWork.Set(cell, giver.def);
			}
			return true;
		}
		return false;
	}

	public bool TryTakeOrderedJob(Job job, JobTag? tag = JobTag.Misc, bool requestQueueing = false)
	{
		if (debugLog)
		{
			DebugLogEvent("TryTakeOrderedJob " + job);
		}
		job.playerForced = true;
		if (curJob != null && curJob.JobIsSameAs(pawn, job))
		{
			return true;
		}
		bool num = pawn.jobs.IsCurrentJobPlayerInterruptible();
		bool flag = pawn.CurJob?.def.forceCompleteBeforeNextJob ?? false;
		bool num2 = num && !flag;
		bool flag2 = pawn.mindState.IsIdle || pawn.CurJob == null || pawn.CurJob.def.isIdle;
		bool isDownEvent = KeyBindingDefOf.QueueOrder.IsDownEvent;
		if (isDownEvent)
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.QueueOrders, KnowledgeAmount.NoteTaught);
		}
		isDownEvent = isDownEvent || requestQueueing;
		if (num2 && (!isDownEvent || flag2))
		{
			if (curJob != null)
			{
				curJob.playerInterruptedForced = true;
			}
			pawn.stances.CancelBusyStanceSoft();
			if (debugLog)
			{
				DebugLogEvent("    Queueing job");
			}
			ClearQueuedJobs();
			if (job.TryMakePreToilReservations(pawn, errorOnFailed: true))
			{
				jobQueue.EnqueueFirst(job, tag);
				if (curJob != null)
				{
					curDriver.EndJobWith(JobCondition.InterruptForced);
				}
				else
				{
					CheckForJobOverride(0f, ignoreQueue: false);
				}
				return true;
			}
			Log.Warning($"TryMakePreToilReservations() returned false right after TryTakeOrderedJob(). This should have been checked before. pawn = {pawn}, job = {job.ToStringSafe()}");
			pawn.ClearReservationsForJob(job);
			return false;
		}
		if (isDownEvent)
		{
			if (job.TryMakePreToilReservations(pawn, errorOnFailed: true))
			{
				jobQueue.EnqueueLast(job, tag);
				return true;
			}
			Log.Warning($"TryMakePreToilReservations() returned false right after TryTakeOrderedJob(). This should have been checked before. pawn = {pawn}, job = {job.ToStringSafe()}");
			pawn.ClearReservationsForJob(job);
			return false;
		}
		ClearQueuedJobs();
		if (job.TryMakePreToilReservations(pawn, errorOnFailed: true))
		{
			jobQueue.EnqueueLast(job, tag);
			return true;
		}
		Log.Warning($"TryMakePreToilReservations() returned false right after TryTakeOrderedJob(). This should have been checked before. pawn = {pawn}, job = {job.ToStringSafe()}");
		pawn.ClearReservationsForJob(job);
		return false;
	}

	public void Notify_TuckedIntoBed(Building_Bed bed)
	{
		pawn.Position = RestUtility.GetBedSleepingSlotPosFor(pawn, bed);
		pawn.Notify_Teleported(endCurrentJob: false);
		pawn.stances.CancelBusyStanceHard();
		JobDef jobDef = (pawn.Deathresting ? JobDefOf.Deathrest : JobDefOf.LayDown);
		StartJob(JobMaker.MakeJob(jobDef, bed), JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, JobTag.TuckedIntoBed, fromQueue: false, canReturnCurJobToPool: false, null, continueSleeping: true);
		if (jobDef == JobDefOf.Deathrest)
		{
			pawn.genes?.GetFirstGeneOfType<Gene_Deathrest>()?.TryLinkToNearbyDeathrestBuildings();
		}
	}

	public void Notify_DamageTaken(DamageInfo dinfo)
	{
		if (curJob == null)
		{
			return;
		}
		Job job = curJob;
		curDriver.Notify_DamageTaken(dinfo);
		if (curJob == job && dinfo.Def.ExternalViolenceFor(pawn) && dinfo.Def.canInterruptJobs && !curJob.playerForced && Find.TickManager.TicksGame >= lastDamageCheckTick + 180)
		{
			Thing instigator = dinfo.Instigator;
			if (curJob.def.checkOverrideOnDamage == CheckJobOverrideOnDamageMode.Always || (curJob.def.checkOverrideOnDamage == CheckJobOverrideOnDamageMode.OnlyIfInstigatorNotJobTarget && !curJob.AnyTargetIs(instigator)))
			{
				lastDamageCheckTick = Find.TickManager.TicksGame;
				CheckForJobOverride();
			}
		}
	}

	internal void Notify_MasterDraftedOrUndrafted()
	{
		Pawn master = pawn.playerSettings.Master;
		if (master.Spawned && master.Map == pawn.Map && pawn.playerSettings.followDrafted)
		{
			EndCurrentJob(JobCondition.InterruptForced);
		}
	}

	public void Notify_MasterStartedFieldWork()
	{
		Pawn master = pawn.playerSettings.Master;
		if (master.Spawned && master.Map == pawn.Map && pawn.playerSettings.followFieldwork && IsCurrentJobPlayerInterruptible())
		{
			EndCurrentJob(JobCondition.InterruptForced);
		}
	}

	public void DrawLinesBetweenTargets()
	{
		Vector3 a = pawn.Position.ToVector3Shifted();
		if (pawn.pather.curPath != null)
		{
			a = pawn.pather.Destination.CenterVector3;
		}
		else if (curJob != null && curJob.def != JobDefOf.LayDown && curJob.targetA.IsValid && (!curJob.targetA.HasThing || (curJob.targetA.Thing.Spawned && curJob.targetA.Thing.Map == pawn.Map)))
		{
			GenDraw.DrawLineBetween(a, curJob.targetA.CenterVector3, AltitudeLayer.Item.AltitudeFor());
			a = curJob.targetA.CenterVector3;
		}
		for (int i = 0; i < jobQueue.Count; i++)
		{
			if (jobQueue[i].job.targetA.IsValid)
			{
				if (!jobQueue[i].job.targetA.HasThing || (jobQueue[i].job.targetA.Thing.Spawned && jobQueue[i].job.targetA.Thing.Map == pawn.Map))
				{
					Vector3 centerVector = jobQueue[i].job.targetA.CenterVector3;
					GenDraw.DrawLineBetween(a, centerVector, AltitudeLayer.Item.AltitudeFor());
					a = centerVector;
				}
				continue;
			}
			List<LocalTargetInfo> targetQueueA = jobQueue[i].job.targetQueueA;
			if (targetQueueA == null)
			{
				continue;
			}
			for (int j = 0; j < targetQueueA.Count; j++)
			{
				if (!targetQueueA[j].HasThing || (targetQueueA[j].Thing.Spawned && targetQueueA[j].Thing.Map == pawn.Map))
				{
					Vector3 centerVector2 = targetQueueA[j].CenterVector3;
					GenDraw.DrawLineBetween(a, centerVector2, AltitudeLayer.Item.AltitudeFor());
					a = centerVector2;
				}
			}
		}
	}

	public void DebugLogEvent(string s)
	{
		if (debugLog)
		{
			Log.Message(Find.TickManager.TicksGame + " " + pawn?.ToString() + ": " + s);
		}
	}

	public void ClearDriver()
	{
		curDriver?.ClearToils();
		curDriver = null;
	}

	private static string CurToilString(JobDriver curDriver)
	{
		if (curDriver == null)
		{
			return "null_driver";
		}
		return $"{curDriver.CurToilString} at toils[{curDriver.CurToilIndex}]";
	}
}
