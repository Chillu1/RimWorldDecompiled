using Verse;

namespace RimWorld
{
	public class QueuedIncident : IExposable
	{
		private FiringIncident firingInc;

		private int fireTick = -1;

		private int retryDurationTicks;

		private bool triedToFire;

		public const int RetryIntervalTicks = 833;

		public int FireTick => fireTick;

		public FiringIncident FiringIncident => firingInc;

		public int RetryDurationTicks => retryDurationTicks;

		public bool TriedToFire => triedToFire;

		public QueuedIncident()
		{
		}

		public QueuedIncident(FiringIncident firingInc, int fireTick, int retryDurationTicks = 0)
		{
			this.firingInc = firingInc;
			this.fireTick = fireTick;
			this.retryDurationTicks = retryDurationTicks;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref firingInc, "firingInc");
			Scribe_Values.Look(ref fireTick, "fireTick", 0);
			Scribe_Values.Look(ref retryDurationTicks, "retryDurationTicks", 0);
			Scribe_Values.Look(ref triedToFire, "triedToFire", defaultValue: false);
		}

		public void Notify_TriedToFire()
		{
			triedToFire = true;
		}

		public override string ToString()
		{
			return fireTick + "->" + firingInc.ToString();
		}
	}
}
