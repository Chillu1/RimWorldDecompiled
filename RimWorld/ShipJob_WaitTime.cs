using Verse;

namespace RimWorld
{
	public class ShipJob_WaitTime : ShipJob_Wait
	{
		public int duration;

		private int startTick = -1;

		protected override bool ShouldEnd
		{
			get
			{
				if (startTick >= 0)
				{
					return Find.TickManager.TicksGame >= startTick + duration;
				}
				return false;
			}
		}

		public override bool TryStart()
		{
			if (!transportShip.ShipExistsAndIsSpawned)
			{
				return false;
			}
			if (!base.TryStart())
			{
				return false;
			}
			if (startTick < 0)
			{
				startTick = Find.TickManager.TicksGame;
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref startTick, "startTick", 0);
			Scribe_Values.Look(ref duration, "duration", 0);
		}
	}
}
