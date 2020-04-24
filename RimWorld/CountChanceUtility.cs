using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class CountChanceUtility
	{
		public static int RandomCount(List<CountChance> chances)
		{
			float value = Rand.Value;
			float num = 0f;
			for (int i = 0; i < chances.Count; i++)
			{
				num += chances[i].chance;
				if (value < num)
				{
					if (num > 1f)
					{
						Log.Error("CountChances error: Total chance is " + num + " but it should not be above 1.");
					}
					return chances[i].count;
				}
			}
			return 0;
		}
	}
}
