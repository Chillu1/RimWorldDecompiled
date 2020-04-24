using System;

namespace Verse
{
	public static class TemperatureDisplayModeExtension
	{
		public static string ToStringHuman(this TemperatureDisplayMode mode)
		{
			switch (mode)
			{
			case TemperatureDisplayMode.Celsius:
				return "Celsius".Translate();
			case TemperatureDisplayMode.Fahrenheit:
				return "Fahrenheit".Translate();
			case TemperatureDisplayMode.Kelvin:
				return "Kelvin".Translate();
			default:
				throw new NotImplementedException();
			}
		}
	}
}
