using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class WeatherPartPool
{
	private static List<SkyOverlay> instances = new List<SkyOverlay>();

	public static SkyOverlay GetInstanceOf<T>() where T : SkyOverlay
	{
		for (int i = 0; i < instances.Count; i++)
		{
			if (instances[i] is T result)
			{
				return result;
			}
		}
		SkyOverlay skyOverlay = Activator.CreateInstance<T>();
		instances.Add(skyOverlay);
		return skyOverlay;
	}
}
