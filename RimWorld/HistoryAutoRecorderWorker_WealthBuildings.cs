using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_WealthBuildings : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			float num = 0f;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome)
				{
					num += maps[i].wealthWatcher.WealthBuildings;
				}
			}
			return num;
		}
	}
}
