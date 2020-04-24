using System;
using Verse;

namespace RimWorld
{
	public static class RestCategoryUtility
	{
		public static string GetLabel(this RestCategory fatigue)
		{
			switch (fatigue)
			{
			case RestCategory.Exhausted:
				return "HungerLevel_Exhausted".Translate();
			case RestCategory.VeryTired:
				return "HungerLevel_VeryTired".Translate();
			case RestCategory.Tired:
				return "HungerLevel_Tired".Translate();
			case RestCategory.Rested:
				return "HungerLevel_Rested".Translate();
			default:
				throw new InvalidOperationException();
			}
		}
	}
}
