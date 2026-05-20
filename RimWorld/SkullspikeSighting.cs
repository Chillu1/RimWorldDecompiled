using Verse;

namespace RimWorld
{
	public struct SkullspikeSighting : IExposable
	{
		public Thing skullspike;

		public int tickSighted;

		public const int MaxSightingAge = 1800;

		public const int CheckRadius = 10;

		public const int CheckIntervalTicks = 60;

		public int TicksSinceSighting => Find.TickManager.TicksGame - tickSighted;

		public SkullspikeSighting(Thing skullspike, int tickSighted)
		{
			this.skullspike = skullspike;
			this.tickSighted = tickSighted;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref skullspike, "skullspike");
			Scribe_Values.Look(ref tickSighted, "tickSighted", 0);
		}
	}
}
