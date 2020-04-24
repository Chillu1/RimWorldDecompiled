using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public sealed class Toil : IJobEndable
	{
		public Pawn actor;

		public Action initAction;

		public Action tickAction;

		public List<Func<JobCondition>> endConditions = new List<Func<JobCondition>>();

		public List<Action> preInitActions;

		public List<Action> preTickActions;

		public List<Action> finishActions;

		public bool atomicWithPrevious;

		public RandomSocialMode socialMode = RandomSocialMode.Normal;

		public Func<SkillDef> activeSkill;

		public ToilCompleteMode defaultCompleteMode = ToilCompleteMode.Instant;

		public int defaultDuration;

		public bool handlingFacing;

		public void Cleanup(int myIndex, JobDriver jobDriver)
		{
			if (finishActions != null)
			{
				for (int i = 0; i < finishActions.Count; i++)
				{
					try
					{
						finishActions[i]();
					}
					catch (Exception ex)
					{
						Log.Error("Pawn " + actor.ToStringSafe() + " threw exception while executing toil's finish action (" + i + "), jobDriver=" + jobDriver.ToStringSafe() + ", job=" + jobDriver.job.ToStringSafe() + ", toilIndex=" + myIndex + ": " + ex);
					}
				}
			}
		}

		public Pawn GetActor()
		{
			return actor;
		}

		public void AddFailCondition(Func<bool> newFailCondition)
		{
			endConditions.Add(() => (!newFailCondition()) ? JobCondition.Ongoing : JobCondition.Incompletable);
		}

		public void AddEndCondition(Func<JobCondition> newEndCondition)
		{
			endConditions.Add(newEndCondition);
		}

		public void AddPreInitAction(Action newAct)
		{
			if (preInitActions == null)
			{
				preInitActions = new List<Action>();
			}
			preInitActions.Add(newAct);
		}

		public void AddPreTickAction(Action newAct)
		{
			if (preTickActions == null)
			{
				preTickActions = new List<Action>();
			}
			preTickActions.Add(newAct);
		}

		public void AddFinishAction(Action newAct)
		{
			if (finishActions == null)
			{
				finishActions = new List<Action>();
			}
			finishActions.Add(newAct);
		}
	}
}
