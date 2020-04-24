using Verse;

namespace RimWorld
{
	public class PawnRecentMemory : IExposable
	{
		private Pawn pawn;

		private int lastLightTick = 999999;

		private int lastOutdoorTick = 999999;

		public int TicksSinceLastLight => Find.TickManager.TicksGame - lastLightTick;

		public int TicksSinceOutdoors => Find.TickManager.TicksGame - lastOutdoorTick;

		public PawnRecentMemory(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref lastLightTick, "lastLightTick", 999999);
			Scribe_Values.Look(ref lastOutdoorTick, "lastOutdoorTick", 999999);
		}

		public void RecentMemoryInterval()
		{
			if (pawn.Spawned)
			{
				if (pawn.Map.glowGrid.PsychGlowAt(pawn.Position) != 0)
				{
					lastLightTick = Find.TickManager.TicksGame;
				}
				if (Outdoors())
				{
					lastOutdoorTick = Find.TickManager.TicksGame;
				}
			}
		}

		private bool Outdoors()
		{
			return pawn.GetRoom()?.PsychologicallyOutdoors ?? false;
		}

		public void Notify_Spawned(bool respawningAfterLoad)
		{
			lastLightTick = Find.TickManager.TicksGame;
			if (!respawningAfterLoad && Outdoors())
			{
				lastOutdoorTick = Find.TickManager.TicksGame;
			}
		}
	}
}
