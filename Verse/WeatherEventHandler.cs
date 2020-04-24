using System.Collections.Generic;

namespace Verse
{
	public class WeatherEventHandler
	{
		private List<WeatherEvent> liveEvents = new List<WeatherEvent>();

		public List<WeatherEvent> LiveEventsListForReading => liveEvents;

		public void AddEvent(WeatherEvent newEvent)
		{
			liveEvents.Add(newEvent);
			newEvent.FireEvent();
		}

		public void WeatherEventHandlerTick()
		{
			for (int num = liveEvents.Count - 1; num >= 0; num--)
			{
				liveEvents[num].WeatherEventTick();
				if (liveEvents[num].Expired)
				{
					liveEvents.RemoveAt(num);
				}
			}
		}

		public void WeatherEventsDraw()
		{
			for (int i = 0; i < liveEvents.Count; i++)
			{
				liveEvents[i].WeatherEventDraw();
			}
		}
	}
}
