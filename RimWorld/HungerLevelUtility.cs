using System;
using Verse;

namespace RimWorld;

public static class HungerLevelUtility
{
	public const float FallPerTickFactor_Hungry = 0.5f;

	public const float FallPerTickFactor_UrgentlyHungry = 0.25f;

	public static string GetLabel(this HungerCategory hunger)
	{
		return hunger switch
		{
			HungerCategory.Starving => "HungerLevel_Starving".Translate(), 
			HungerCategory.UrgentlyHungry => "HungerLevel_UrgentlyHungry".Translate(), 
			HungerCategory.Hungry => "HungerLevel_Hungry".Translate(), 
			HungerCategory.Fed => "HungerLevel_Fed".Translate(), 
			_ => throw new InvalidOperationException(), 
		};
	}

	public static float HungerMultiplier(this HungerCategory cat)
	{
		return cat switch
		{
			HungerCategory.Fed => 1f, 
			HungerCategory.Hungry => 0.5f, 
			HungerCategory.UrgentlyHungry => 0.25f, 
			HungerCategory.Starving => 0f, 
			_ => throw new NotImplementedException(), 
		};
	}
}
