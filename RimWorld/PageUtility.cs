using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PageUtility
	{
		public static Page StitchedPages(IEnumerable<Page> pages)
		{
			List<Page> list = pages.ToList();
			if (list.Count == 0)
			{
				return null;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					list[i].prev = list[i - 1];
				}
				if (i < list.Count - 1)
				{
					list[i].next = list[i + 1];
				}
			}
			return list[0];
		}

		public static void InitGameStart()
		{
			LongEventHandler.QueueLongEvent(delegate
			{
				Find.GameInitData.PrepForMapGen();
				Find.GameInitData.startedFromEntry = true;
				Find.Scenario.PreMapGenerate();
			}, "Play", "GeneratingMap", doAsynchronously: true, null);
		}
	}
}
