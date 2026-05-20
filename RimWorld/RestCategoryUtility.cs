using System;
using Verse;

namespace RimWorld;

public static class RestCategoryUtility
{
	public static string GetLabel(this RestCategory fatigue)
	{
		return fatigue switch
		{
			RestCategory.Exhausted => "HungerLevel_Exhausted".Translate(), 
			RestCategory.VeryTired => "HungerLevel_VeryTired".Translate(), 
			RestCategory.Tired => "HungerLevel_Tired".Translate(), 
			RestCategory.Rested => "HungerLevel_Rested".Translate(), 
			_ => throw new InvalidOperationException(), 
		};
	}
}
