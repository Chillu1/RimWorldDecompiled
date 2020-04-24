using System;

namespace Verse
{
	public class WeatherEventMaker
	{
		public float averageInterval = 100f;

		public Type eventClass;

		public void WeatherEventMakerTick(Map map, float strength)
		{
			if (Rand.Value < 1f / averageInterval * strength)
			{
				WeatherEvent newEvent = (WeatherEvent)Activator.CreateInstance(eventClass, map);
				map.weatherManager.eventHandler.AddEvent(newEvent);
			}
		}
	}
}
