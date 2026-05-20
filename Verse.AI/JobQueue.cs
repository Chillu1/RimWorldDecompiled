using System;
using System.Collections;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobQueue : IExposable, IEnumerable<QueuedJob>, IEnumerable
{
	private List<QueuedJob> jobs = new List<QueuedJob>();

	public int Count => jobs.Count;

	public QueuedJob this[int index] => jobs[index];

	public bool AnyPlayerForced
	{
		get
		{
			for (int i = 0; i < jobs.Count; i++)
			{
				if (jobs[i].job.playerForced)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref jobs, "jobs", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			jobs.RemoveAll((QueuedJob j) => j.job?.def == null);
		}
	}

	public void EnqueueFirst(Job j, JobTag? tag = null)
	{
		jobs.Insert(0, new QueuedJob(j, tag));
	}

	public void EnqueueLast(Job j, JobTag? tag = null)
	{
		jobs.Add(new QueuedJob(j, tag));
	}

	public void Clear(Pawn pawn, bool canReturnToPool)
	{
		for (int i = 0; i < jobs.Count; i++)
		{
			QueuedJob queuedJob = jobs[i];
			jobs[i] = null;
			queuedJob.Cleanup(pawn, canReturnToPool);
		}
		jobs.Clear();
	}

	public bool Contains(Job j)
	{
		for (int i = 0; i < jobs.Count; i++)
		{
			if (jobs[i]?.job == j)
			{
				return true;
			}
		}
		return false;
	}

	public QueuedJob Extract(Job j)
	{
		int num = jobs.FindIndex((QueuedJob qj) => qj.job == j);
		if (num >= 0)
		{
			QueuedJob result = jobs[num];
			jobs.RemoveAt(num);
			return result;
		}
		return null;
	}

	public QueuedJob Dequeue()
	{
		if (jobs.NullOrEmpty())
		{
			return null;
		}
		QueuedJob result = jobs[0];
		jobs.RemoveAt(0);
		return result;
	}

	public QueuedJob Peek()
	{
		return jobs[0];
	}

	public bool AnyCanBeginNow(Pawn pawn, bool whileLyingDown)
	{
		for (int i = 0; i < jobs.Count; i++)
		{
			if (jobs[i].job.CanBeginNow(pawn, whileLyingDown))
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerator<QueuedJob> GetEnumerator()
	{
		return jobs.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return jobs.GetEnumerator();
	}

	public JobQueue Capture()
	{
		JobQueue jobQueue = new JobQueue();
		foreach (QueuedJob job in jobs)
		{
			jobQueue.jobs.Add(new QueuedJob(job));
		}
		return jobQueue;
	}

	public void RemoveAll(Pawn pawn, Predicate<Job> filter)
	{
		for (int i = 0; i < jobs.Count; i++)
		{
			QueuedJob queuedJob = jobs[i];
			if (filter(queuedJob.job))
			{
				jobs[i] = null;
				queuedJob.Cleanup(pawn, canReturnToPool: true);
			}
		}
		jobs.RemoveAll((QueuedJob job) => job == null);
	}

	public void RemoveAllWorkType(Pawn pawn, WorkTypeDef wType, bool hardDisable)
	{
		for (int i = 0; i < jobs.Count; i++)
		{
			QueuedJob queuedJob = jobs[i];
			if (queuedJob.job.workGiverDef != null && queuedJob.job.workGiverDef.workType == wType && (hardDisable || !queuedJob.job.playerForced))
			{
				jobs[i] = null;
				queuedJob.Cleanup(pawn, canReturnToPool: true);
			}
		}
		jobs.RemoveAll((QueuedJob job) => job == null);
	}

	public void RemoveAllJoyKind(Pawn pawn, JoyKindDef joyKind)
	{
		for (int i = 0; i < jobs.Count; i++)
		{
			QueuedJob queuedJob = jobs[i];
			if (queuedJob.job.def.joyKind == joyKind)
			{
				jobs[i] = null;
				queuedJob.Cleanup(pawn, canReturnToPool: true);
			}
		}
		jobs.RemoveAll((QueuedJob job) => job == null);
	}
}
