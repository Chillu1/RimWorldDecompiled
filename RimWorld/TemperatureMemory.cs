using Verse;

namespace RimWorld
{
	public class TemperatureMemory : IExposable
	{
		private Map map;

		private int growthSeasonUntilTick = -1;

		private int noSowUntilTick = -1;

		private const int TicksBuffer = 30000;

		public bool GrowthSeasonOutdoorsNow => Find.TickManager.TicksGame < growthSeasonUntilTick;

		public bool GrowthSeasonOutdoorsNowForSowing
		{
			get
			{
				if (noSowUntilTick > 0 && Find.TickManager.TicksGame < noSowUntilTick)
				{
					return false;
				}
				return GrowthSeasonOutdoorsNow;
			}
		}

		public TemperatureMemory(Map map)
		{
			this.map = map;
		}

		public void GrowthSeasonMemoryTick()
		{
			if (map.mapTemperature.OutdoorTemp > 0f && map.mapTemperature.OutdoorTemp < 58f)
			{
				growthSeasonUntilTick = Find.TickManager.TicksGame + 30000;
			}
			else if (map.mapTemperature.OutdoorTemp < -2f)
			{
				growthSeasonUntilTick = -1;
				noSowUntilTick = Find.TickManager.TicksGame + 30000;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref growthSeasonUntilTick, "growthSeasonUntilTick", 0, forceSave: true);
			Scribe_Values.Look(ref noSowUntilTick, "noSowUntilTick", 0, forceSave: true);
		}
	}
}
