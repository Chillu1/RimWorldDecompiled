namespace Verse
{
	public abstract class Stance : IExposable
	{
		public Pawn_StanceTracker stanceTracker;

		public virtual bool StanceBusy => false;

		protected Pawn Pawn => stanceTracker.pawn;

		public virtual void StanceTick()
		{
		}

		public virtual void StanceDraw()
		{
		}

		public virtual void ExposeData()
		{
		}
	}
}
