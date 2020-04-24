using Verse.AI;

namespace Verse
{
	public static class JobMaker
	{
		private const int MaxJobPoolSize = 1000;

		public static Job MakeJob()
		{
			Job job = SimplePool<Job>.Get();
			job.loadID = Find.UniqueIDsManager.GetNextJobID();
			return job;
		}

		public static Job MakeJob(JobDef def)
		{
			Job job = MakeJob();
			job.def = def;
			return job;
		}

		public static Job MakeJob(JobDef def, LocalTargetInfo targetA)
		{
			Job job = MakeJob();
			job.def = def;
			job.targetA = targetA;
			return job;
		}

		public static Job MakeJob(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB)
		{
			Job job = MakeJob();
			job.def = def;
			job.targetA = targetA;
			job.targetB = targetB;
			return job;
		}

		public static Job MakeJob(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB, LocalTargetInfo targetC)
		{
			Job job = MakeJob();
			job.def = def;
			job.targetA = targetA;
			job.targetB = targetB;
			job.targetC = targetC;
			return job;
		}

		public static Job MakeJob(JobDef def, LocalTargetInfo targetA, int expiryInterval, bool checkOverrideOnExpiry = false)
		{
			Job job = MakeJob();
			job.def = def;
			job.targetA = targetA;
			job.expiryInterval = expiryInterval;
			job.checkOverrideOnExpire = checkOverrideOnExpiry;
			return job;
		}

		public static Job MakeJob(JobDef def, int expiryInterval, bool checkOverrideOnExpiry = false)
		{
			Job job = MakeJob();
			job.def = def;
			job.expiryInterval = expiryInterval;
			job.checkOverrideOnExpire = checkOverrideOnExpiry;
			return job;
		}

		public static void ReturnToPool(Job job)
		{
			if (job != null && SimplePool<Job>.FreeItemsCount < 1000)
			{
				job.Clear();
				SimplePool<Job>.Return(job);
			}
		}
	}
}
