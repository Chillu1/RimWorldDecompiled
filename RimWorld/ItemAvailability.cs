using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ItemAvailability
{
	private readonly Map map;

	private readonly Dictionary<int, bool> cachedResults = new Dictionary<int, bool>();

	public ItemAvailability(Map map)
	{
		this.map = map;
	}

	public void Tick()
	{
		cachedResults.Clear();
	}

	public bool ThingsAvailableAnywhere(ThingDef need, int amount, Pawn pawn)
	{
		int key = Gen.HashCombine(need.GetHashCode(), pawn.Faction);
		if (!cachedResults.TryGetValue(key, out var value))
		{
			List<Thing> list = map.listerThings.ThingsOfDef(need);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].IsForbidden(pawn))
				{
					num += list[i].stackCount;
					if (num >= amount)
					{
						break;
					}
				}
			}
			value = num >= amount;
			cachedResults.Add(key, value);
		}
		return value;
	}
}
