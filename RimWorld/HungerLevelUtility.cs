using System;
using Verse;

namespace RimWorld
{
	public static class HungerLevelUtility
	{
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
	}
}
