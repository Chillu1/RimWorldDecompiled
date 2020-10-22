using System.Collections;
using System.Collections.Generic;

namespace Verse.AI
{
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
		}

		public void EnqueueFirst(Job j, JobTag? tag = null)
		{
			jobs.Insert(0, new QueuedJob(j, tag));
		}

		public void EnqueueLast(Job j, JobTag? tag = null)
		{
			jobs.Add(new QueuedJob(j, tag));
		}

		public bool Contains(Job j)
		{
			for (int i = 0; i < jobs.Count; i++)
			{
				if (jobs[i].job == j)
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
			jobQueue.jobs.AddRange(jobs);
			return jobQueue;
		}
	}
}
