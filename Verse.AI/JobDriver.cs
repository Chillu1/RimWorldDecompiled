using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public abstract class JobDriver : IExposable, IJobEndable
	{
		public Pawn pawn;

		public Job job;

		private List<Toil> toils = new List<Toil>();

		public List<Func<JobCondition>> globalFailConditions = new List<Func<JobCondition>>();

		public List<Action> globalFinishActions = new List<Action>();

		public bool ended;

		private int curToilIndex = -1;

		private ToilCompleteMode curToilCompleteMode;

		public int ticksLeftThisToil = 99999;

		private bool wantBeginNextToil;

		protected int startTick = -1;

		public TargetIndex rotateToFace = TargetIndex.A;

		private int nextToilIndex = -1;

		public bool asleep;

		public float uninstallWorkLeft;

		public bool collideWithPawns;

		public Pawn locomotionUrgencySameAs;

		public int debugTicksSpentThisToil;

		protected Toil CurToil
		{
			get
			{
				if (curToilIndex < 0 || job == null || pawn.CurJob != job)
				{
					return null;
				}
				if (curToilIndex >= toils.Count)
				{
					Log.Error(pawn + " with job " + pawn.CurJob + " tried to get CurToil with curToilIndex=" + curToilIndex + " but only has " + toils.Count + " toils.");
					return null;
				}
				return toils[curToilIndex];
			}
		}

		protected bool HaveCurToil
		{
			get
			{
				if (curToilIndex >= 0 && curToilIndex < toils.Count && job != null)
				{
					return pawn.CurJob == job;
				}
				return false;
			}
		}

		private bool CanStartNextToilInBusyStance
		{
			get
			{
				int num = curToilIndex + 1;
				if (num >= toils.Count)
				{
					return false;
				}
				return toils[num].atomicWithPrevious;
			}
		}

		public int CurToilIndex => curToilIndex;

		public bool OnLastToil => CurToilIndex == toils.Count - 1;

		public SkillDef ActiveSkill
		{
			get
			{
				if (!HaveCurToil || CurToil.activeSkill == null)
				{
					return null;
				}
				return CurToil.activeSkill();
			}
		}

		public bool HandlingFacing
		{
			get
			{
				if (CurToil != null)
				{
					return CurToil.handlingFacing;
				}
				return false;
			}
		}

		protected LocalTargetInfo TargetA => job.targetA;

		protected LocalTargetInfo TargetB => job.targetB;

		protected LocalTargetInfo TargetC => job.targetC;

		protected Thing TargetThingA
		{
			get
			{
				return job.targetA.Thing;
			}
			set
			{
				job.targetA = value;
			}
		}

		protected Thing TargetThingB
		{
			get
			{
				return job.targetB.Thing;
			}
			set
			{
				job.targetB = value;
			}
		}

		protected IntVec3 TargetLocA => job.targetA.Cell;

		protected Map Map => pawn.Map;

		public virtual string GetReport()
		{
			return ReportStringProcessed(job.def.reportString);
		}

		protected string ReportStringProcessed(string str)
		{
			LocalTargetInfo a = job.targetA.IsValid ? job.targetA : job.targetQueueA.FirstValid();
			LocalTargetInfo b = job.targetB.IsValid ? job.targetB : job.targetQueueB.FirstValid();
			LocalTargetInfo targetC = job.targetC;
			return JobUtility.GetResolvedJobReport(str, a, b, targetC);
		}

		public abstract bool TryMakePreToilReservations(bool errorOnFailed);

		protected abstract IEnumerable<Toil> MakeNewToils();

		public virtual void SetInitialPosture()
		{
			pawn.jobs.posture = PawnPosture.Standing;
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref ended, "ended", defaultValue: false);
			Scribe_Values.Look(ref curToilIndex, "curToilIndex", 0, forceSave: true);
			Scribe_Values.Look(ref ticksLeftThisToil, "ticksLeftThisToil", 0);
			Scribe_Values.Look(ref wantBeginNextToil, "wantBeginNextToil", defaultValue: false);
			Scribe_Values.Look(ref curToilCompleteMode, "curToilCompleteMode", ToilCompleteMode.Undefined);
			Scribe_Values.Look(ref startTick, "startTick", 0);
			Scribe_Values.Look(ref rotateToFace, "rotateToFace", TargetIndex.A);
			Scribe_Values.Look(ref asleep, "asleep", defaultValue: false);
			Scribe_Values.Look(ref uninstallWorkLeft, "uninstallWorkLeft", 0f);
			Scribe_Values.Look(ref nextToilIndex, "nextToilIndex", -1);
			Scribe_Values.Look(ref collideWithPawns, "collideWithPawns", defaultValue: false);
			Scribe_References.Look(ref locomotionUrgencySameAs, "locomotionUrgencySameAs");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				SetupToils();
			}
		}

		public void Cleanup(JobCondition condition)
		{
			for (int i = 0; i < globalFinishActions.Count; i++)
			{
				try
				{
					globalFinishActions[i]();
				}
				catch (Exception ex)
				{
					Log.Error("Pawn " + pawn.ToStringSafe() + " threw exception while executing a global finish action (" + i + "), jobDriver=" + this.ToStringSafe() + ", job=" + job.ToStringSafe() + ": " + ex);
				}
			}
			if (curToilIndex >= 0 && curToilIndex < toils.Count)
			{
				toils[curToilIndex].Cleanup(curToilIndex, this);
			}
		}

		public virtual bool CanBeginNowWhileLyingDown()
		{
			return false;
		}

		internal void SetupToils()
		{
			try
			{
				toils.Clear();
				foreach (Toil item in MakeNewToils())
				{
					if (item.defaultCompleteMode == ToilCompleteMode.Undefined)
					{
						Log.Error("Toil has undefined complete mode.");
						item.defaultCompleteMode = ToilCompleteMode.Instant;
					}
					item.actor = pawn;
					toils.Add(item);
				}
			}
			catch (Exception exception)
			{
				JobUtility.TryStartErrorRecoverJob(pawn, "Exception in SetupToils for pawn " + pawn.ToStringSafe(), exception, this);
			}
		}

		public void DriverTick()
		{
			try
			{
				ticksLeftThisToil--;
				debugTicksSpentThisToil++;
				if (CurToil == null)
				{
					if (!pawn.stances.FullBodyBusy || CanStartNextToilInBusyStance)
					{
						ReadyForNextToil();
					}
				}
				else if (!CheckCurrentToilEndOrFail())
				{
					if (curToilCompleteMode == ToilCompleteMode.Delay)
					{
						if (ticksLeftThisToil > 0)
						{
							goto IL_0099;
						}
						ReadyForNextToil();
					}
					else
					{
						if (curToilCompleteMode != ToilCompleteMode.FinishedBusy || pawn.stances.FullBodyBusy)
						{
							goto IL_0099;
						}
						ReadyForNextToil();
					}
				}
				goto end_IL_0000;
				IL_01b8:
				if (job.mote != null)
				{
					job.mote.Maintain();
				}
				goto end_IL_0000;
				IL_0099:
				Job startingJob;
				int startingJobId;
				if (wantBeginNextToil)
				{
					TryActuallyStartNextToil();
				}
				else if (curToilCompleteMode == ToilCompleteMode.Instant && debugTicksSpentThisToil > 300)
				{
					Log.Error(pawn + " had to be broken from frozen state. He was doing job " + job + ", toilindex=" + curToilIndex);
					ReadyForNextToil();
				}
				else
				{
					startingJob = pawn.CurJob;
					startingJobId = startingJob.loadID;
					if (CurToil.preTickActions != null)
					{
						Toil curToil = CurToil;
						for (int i = 0; i < curToil.preTickActions.Count; i++)
						{
							curToil.preTickActions[i]();
							if (JobChanged() || CurToil != curToil || wantBeginNextToil)
							{
								return;
							}
						}
					}
					if (CurToil.tickAction == null)
					{
						goto IL_01b8;
					}
					CurToil.tickAction();
					if (!JobChanged())
					{
						goto IL_01b8;
					}
				}
				end_IL_0000:
				bool JobChanged()
				{
					if (pawn.CurJob == startingJob)
					{
						return pawn.CurJob.loadID != startingJobId;
					}
					return true;
				}
			}
			catch (Exception exception)
			{
				JobUtility.TryStartErrorRecoverJob(pawn, "Exception in JobDriver tick for pawn " + pawn.ToStringSafe(), exception, this);
			}
		}

		public void ReadyForNextToil()
		{
			wantBeginNextToil = true;
			TryActuallyStartNextToil();
		}

		private void TryActuallyStartNextToil()
		{
			if (!pawn.Spawned || (pawn.stances.FullBodyBusy && !CanStartNextToilInBusyStance) || job == null || pawn.CurJob != job)
			{
				return;
			}
			if (HaveCurToil)
			{
				CurToil.Cleanup(curToilIndex, this);
			}
			if (nextToilIndex >= 0)
			{
				curToilIndex = nextToilIndex;
				nextToilIndex = -1;
			}
			else
			{
				curToilIndex++;
			}
			wantBeginNextToil = false;
			if (!HaveCurToil)
			{
				if (pawn.stances != null && pawn.stances.curStance.StanceBusy)
				{
					Log.ErrorOnce(pawn.ToStringSafe() + " ended job " + job.ToStringSafe() + " due to running out of toils during a busy stance.", 6453432);
				}
				EndJobWith(JobCondition.Succeeded);
				return;
			}
			debugTicksSpentThisToil = 0;
			ticksLeftThisToil = CurToil.defaultDuration;
			curToilCompleteMode = CurToil.defaultCompleteMode;
			if (CheckCurrentToilEndOrFail())
			{
				return;
			}
			Toil curToil = CurToil;
			if (CurToil.preInitActions != null)
			{
				for (int i = 0; i < CurToil.preInitActions.Count; i++)
				{
					try
					{
						CurToil.preInitActions[i]();
					}
					catch (Exception exception)
					{
						JobUtility.TryStartErrorRecoverJob(pawn, "JobDriver threw exception in preInitActions[" + i + "] for pawn " + pawn.ToStringSafe(), exception, this);
						return;
					}
					if (CurToil != curToil)
					{
						break;
					}
				}
			}
			if (CurToil == curToil)
			{
				if (CurToil.initAction != null)
				{
					try
					{
						CurToil.initAction();
					}
					catch (Exception exception2)
					{
						JobUtility.TryStartErrorRecoverJob(pawn, "JobDriver threw exception in initAction for pawn " + pawn.ToStringSafe(), exception2, this);
						return;
					}
				}
				if (!ended && curToilCompleteMode == ToilCompleteMode.Instant && CurToil == curToil)
				{
					ReadyForNextToil();
				}
			}
		}

		public void EndJobWith(JobCondition condition)
		{
			if (!pawn.Destroyed && job != null && pawn.CurJob == job)
			{
				pawn.jobs.EndCurrentJob(condition);
			}
		}

		public virtual object[] TaleParameters()
		{
			return new object[1]
			{
				pawn
			};
		}

		private bool CheckCurrentToilEndOrFail()
		{
			try
			{
				Toil curToil = CurToil;
				if (globalFailConditions != null)
				{
					for (int i = 0; i < globalFailConditions.Count; i++)
					{
						JobCondition jobCondition = globalFailConditions[i]();
						if (jobCondition != JobCondition.Ongoing)
						{
							if (pawn.jobs.debugLog)
							{
								pawn.jobs.DebugLogEvent(GetType().Name + " ends current job " + job.ToStringSafe() + " because of globalFailConditions[" + i + "]");
							}
							EndJobWith(jobCondition);
							return true;
						}
					}
				}
				if (curToil != null && curToil.endConditions != null)
				{
					for (int j = 0; j < curToil.endConditions.Count; j++)
					{
						JobCondition jobCondition2 = curToil.endConditions[j]();
						if (jobCondition2 != JobCondition.Ongoing)
						{
							if (pawn.jobs.debugLog)
							{
								pawn.jobs.DebugLogEvent(GetType().Name + " ends current job " + job.ToStringSafe() + " because of toils[" + curToilIndex + "].endConditions[" + j + "]");
							}
							EndJobWith(jobCondition2);
							return true;
						}
					}
				}
				return false;
			}
			catch (Exception exception)
			{
				JobUtility.TryStartErrorRecoverJob(pawn, "Exception in CheckCurrentToilEndOrFail for pawn " + pawn.ToStringSafe(), exception, this);
				return true;
			}
		}

		private void SetNextToil(Toil to)
		{
			if (to != null && !toils.Contains(to))
			{
				Log.Warning("SetNextToil with non-existent toil (" + to.ToStringSafe() + "). pawn=" + pawn.ToStringSafe() + ", job=" + pawn.CurJob.ToStringSafe());
			}
			nextToilIndex = toils.IndexOf(to);
		}

		public void JumpToToil(Toil to)
		{
			if (to == null)
			{
				Log.Warning("JumpToToil with null toil. pawn=" + pawn.ToStringSafe() + ", job=" + pawn.CurJob.ToStringSafe());
			}
			SetNextToil(to);
			ReadyForNextToil();
		}

		public virtual void Notify_Starting()
		{
			startTick = Find.TickManager.TicksGame;
		}

		public virtual void Notify_PatherArrived()
		{
			if (curToilCompleteMode == ToilCompleteMode.PatherArrival)
			{
				ReadyForNextToil();
			}
		}

		public virtual void Notify_PatherFailed()
		{
			EndJobWith(JobCondition.ErroredPather);
		}

		public virtual void Notify_StanceChanged()
		{
		}

		public virtual void Notify_DamageTaken(DamageInfo dinfo)
		{
		}

		public Pawn GetActor()
		{
			return pawn;
		}

		public void AddEndCondition(Func<JobCondition> newEndCondition)
		{
			globalFailConditions.Add(newEndCondition);
		}

		public void AddFailCondition(Func<bool> newFailCondition)
		{
			globalFailConditions.Add(() => (!newFailCondition()) ? JobCondition.Ongoing : JobCondition.Incompletable);
		}

		public void AddFinishAction(Action newAct)
		{
			globalFinishActions.Add(newAct);
		}

		public virtual bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			return false;
		}

		public virtual RandomSocialMode DesiredSocialMode()
		{
			if (CurToil != null)
			{
				return CurToil.socialMode;
			}
			return RandomSocialMode.Normal;
		}

		public virtual bool IsContinuation(Job j)
		{
			return true;
		}
	}
}
