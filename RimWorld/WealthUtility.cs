using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class WealthUtility
	{
		public static float PlayerWealth
		{
			get
			{
				List<Map> maps = Find.Maps;
				float num = 0f;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].IsPlayerHome)
					{
						num += maps[i].wealthWatcher.WealthTotal;
					}
				}
				return num;
			}
		}
	}
}
