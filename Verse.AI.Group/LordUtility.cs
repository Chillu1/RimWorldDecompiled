using System.Collections.Generic;

namespace Verse.AI.Group;

public static class LordUtility
{
	public static Lord GetLord(this Pawn p)
	{
		return p.lord;
	}

	public static bool TryGetLord(this Pawn p, out Lord lord)
	{
		lord = p.GetLord();
		return lord != null;
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

	public static Lord GetLord(this Corpse c)
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			Lord lord = maps[i].lordManager.LordOf(c);
			if (lord != null)
			{
				return lord;
			}
		}
		return null;
	}
}
