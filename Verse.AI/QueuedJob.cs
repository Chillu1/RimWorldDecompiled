namespace Verse.AI
{
	public class QueuedJob : IExposable
	{
		public Job job;

		public JobTag? tag;

		public QueuedJob()
		{
		}

		public QueuedJob(Job job, JobTag? tag)
		{
			this.job = job;
			this.tag = tag;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref job, "job");
			Scribe_Values.Look(ref tag, "tag");
		}
	}
}
