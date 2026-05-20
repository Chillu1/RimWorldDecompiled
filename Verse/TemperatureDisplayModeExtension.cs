using System;

namespace Verse;

public static class TemperatureDisplayModeExtension
{
	public static string ToStringHuman(this TemperatureDisplayMode mode)
	{
		return mode switch
		{
			TemperatureDisplayMode.Celsius => "Celsius".Translate(), 
			TemperatureDisplayMode.Fahrenheit => "Fahrenheit".Translate(), 
			TemperatureDisplayMode.Kelvin => "Kelvin".Translate(), 
			_ => throw new NotImplementedException(), 
		};
	}
}
