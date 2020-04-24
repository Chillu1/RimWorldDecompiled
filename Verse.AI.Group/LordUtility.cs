using System.Collections.Generic;

namespace Verse.AI.Group
{
	public static class LordUtility
	{
		public static Lord GetLord(this Pawn p)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Lord lord = maps[i].lordManager.LordOf(p);
				if (lord != null)
				{
					return lord;
				}
			}
			return null;
		}

		public static Lord GetLord(this Building b)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Lord lord = maps[i].lordManager.LordOf(b);
				if (lord != null)
				{
					return lord;
				}
			}
			return null;
		}
	}
}
