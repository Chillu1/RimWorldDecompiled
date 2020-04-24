using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class WeatherPartPool
	{
		private static List<SkyOverlay> instances = new List<SkyOverlay>();

		public static SkyOverlay GetInstanceOf<T>() where T : SkyOverlay
		{
			for (int i = 0; i < instances.Count; i++)
			{
				T val = instances[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			SkyOverlay skyOverlay = Activator.CreateInstance<T>();
			instances.Add(skyOverlay);
			return skyOverlay;
		}
	}
}
