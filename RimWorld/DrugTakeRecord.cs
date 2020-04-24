using Verse;

namespace RimWorld
{
	public class DrugTakeRecord : IExposable
	{
		public ThingDef drug;

		public int lastTakenTicks;

		private int timesTakenThisDayInt;

		private int thisDay;

		public int LastTakenDays => GenDate.DaysPassedAt(lastTakenTicks);

		public int TimesTakenThisDay
		{
			get
			{
				if (thisDay != GenDate.DaysPassed)
				{
					return 0;
				}
				return timesTakenThisDayInt;
			}
			set
			{
				timesTakenThisDayInt = value;
				thisDay = GenDate.DaysPassed;
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref drug, "drug");
			Scribe_Values.Look(ref lastTakenTicks, "lastTakenTicks", 0);
			Scribe_Values.Look(ref timesTakenThisDayInt, "timesTakenThisDay", 0);
			Scribe_Values.Look(ref thisDay, "thisDay", 0);
		}
	}
}
