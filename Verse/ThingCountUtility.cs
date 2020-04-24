using System.Collections.Generic;

namespace Verse
{
	public static class ThingCountUtility
	{
		public static int CountOf(List<ThingCount> list, Thing thing)
		{
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Thing == thing)
				{
					num += list[i].Count;
				}
			}
			return num;
		}

		public static void AddToList(List<ThingCount> list, Thing thing, int countToAdd)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Thing == thing)
				{
					list[i] = list[i].WithCount(list[i].Count + countToAdd);
					return;
				}
			}
			list.Add(new ThingCount(thing, countToAdd));
		}
	}
}
