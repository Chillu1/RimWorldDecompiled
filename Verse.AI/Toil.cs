using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public sealed class Toil : IJobEndable
{
	public Pawn actor;

	public Action initAction;

	public Action tickAction;

	public Action<int> tickIntervalAction;

	public List<Func<JobCondition>> endConditions = new List<Func<JobCondition>>();

	public List<Action> preInitActions;

	public List<Action> preTickActions;

	public List<Action<int>> preTickIntervalActions;

	public List<Action> finishActions;

	public bool atomicWithPrevious;

	public RandomSocialMode socialMode = RandomSocialMode.Normal;

	public Func<SkillDef> activeSkill;

	public ToilCompleteMode defaultCompleteMode = ToilCompleteMode.Instant;

	public int defaultDuration;

	public bool handlingFacing;

	public bool inPool = true;

	public string debugName;

	public void Clear()
	{
		initAction = null;
		tickAction = null;
		tickIntervalAction = null;
		endConditions.Clear();
		preInitActions = null;
		preTickActions = null;
		preTickIntervalActions = null;
		finishActions = null;
		atomicWithPrevious = false;
		socialMode = RandomSocialMode.Normal;
		activeSkill = null;
		defaultCompleteMode = ToilCompleteMode.Instant;
		defaultDuration = 0;
		handlingFacing = false;
		debugName = null;
	}

	public void Cleanup(int myIndex, JobDriver jobDriver)
	{
		if (finishActions == null)
		{
			return;
		}
		for (int i = 0; i < finishActions.Count; i++)
		{
			try
			{
				finishActions[i]();
			}
			catch (Exception ex)
			{
				Log.Error($"Pawn {actor.ToStringSafe()} threw exception while executing toil {this}'s finish action ({i})" + ", jobDriver=" + jobDriver.ToStringSafe() + ", job=" + jobDriver.job.ToStringSafe() + ", toilIndex=" + myIndex + ": " + ex);
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

	public void AddPreTickIntervalAction(Action<int> newAct)
	{
		if (preTickIntervalActions == null)
		{
			preTickIntervalActions = new List<Action<int>>();
		}
		preTickIntervalActions.Add(newAct);
	}

	public void AddFinishAction(Action newAct)
	{
		if (finishActions == null)
		{
			finishActions = new List<Action>();
		}
		finishActions.Add(newAct);
	}

	public override string ToString()
	{
		return debugName ?? "unnamed";
	}
}
