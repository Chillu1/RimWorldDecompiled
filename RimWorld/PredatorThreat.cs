using Verse;

namespace RimWorld
{
	public class PredatorThreat : IExposable
	{
		public Pawn predator;

		public int lastAttackTicks;

		private const int ExpireAfterTicks = 600;

		public bool Expired
		{
			get
			{
				if (!predator.Spawned)
				{
					return true;
				}
				return Find.TickManager.TicksGame >= lastAttackTicks + 600;
			}
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref predator, "predator");
			Scribe_Values.Look(ref lastAttackTicks, "lastAttackTicks", 0);
		}
	}
}
