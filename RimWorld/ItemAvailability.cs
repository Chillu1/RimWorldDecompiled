using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ItemAvailability
	{
		private Map map;

		private Dictionary<int, bool> cachedResults = new Dictionary<int, bool>();

		public ItemAvailability(Map map)
		{
			this.map = map;
		}

		public void Tick()
		{
			cachedResults.Clear();
		}

		public bool ThingsAvailableAnywhere(ThingDefCountClass need, Pawn pawn)
		{
			int key = Gen.HashCombine(need.GetHashCode(), pawn.Faction);
			if (!cachedResults.TryGetValue(key, out var value))
			{
				List<Thing> list = map.listerThings.ThingsOfDef(need.thingDef);
				int num = 0;
				for (int i = 0; i < list.Count; i++)
				{
					if (!list[i].IsForbidden(pawn))
					{
						num += list[i].stackCount;
						if (num >= need.count)
						{
							break;
						}
					}
				}
				value = num >= need.count;
				cachedResults.Add(key, value);
			}
			return value;
		}
	}
}
