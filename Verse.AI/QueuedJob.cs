namespace Verse.AI
{
	public class QueuedJob : IExposable
	{
		public Job job;

		public JobTag? tag;

		public QueuedJob()
		{
		}

		public QueuedJob(QueuedJob other)
			: this(other.job, other.tag)
		{
		}

		public QueuedJob(Job job, JobTag? tag)
		{
			this.job = job;
			this.tag = tag;
		}

		public void Cleanup(Pawn pawn, bool canReturnToPool)
		{
			pawn?.ClearReservationsForJob(job);
			if (canReturnToPool)
			{
				JobMaker.ReturnToPool(job);
			}
			job = null;
			tag = null;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref job, "job");
			Scribe_Values.Look(ref tag, "tag");
		}
	}
}
