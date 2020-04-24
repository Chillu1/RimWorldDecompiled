using System;
using System.Collections.Generic;

namespace Verse
{
	public class TickList
	{
		private TickerType tickType;

		private List<List<Thing>> thingLists = new List<List<Thing>>();

		private List<Thing> thingsToRegister = new List<Thing>();

		private List<Thing> thingsToDeregister = new List<Thing>();

		private int TickInterval
		{
			get
			{
				switch (tickType)
				{
				case TickerType.Normal:
					return 1;
				case TickerType.Rare:
					return 250;
				case TickerType.Long:
					return 2000;
				default:
					return -1;
				}
			}
		}

		public TickList(TickerType tickType)
		{
			this.tickType = tickType;
			for (int i = 0; i < TickInterval; i++)
			{
				thingLists.Add(new List<Thing>());
			}
		}

		public void Reset()
		{
			for (int i = 0; i < thingLists.Count; i++)
			{
				thingLists[i].Clear();
			}
			thingsToRegister.Clear();
			thingsToDeregister.Clear();
		}

		public void RemoveWhere(Predicate<Thing> predicate)
		{
			for (int i = 0; i < thingLists.Count; i++)
			{
				thingLists[i].RemoveAll(predicate);
			}
			thingsToRegister.RemoveAll(predicate);
			thingsToDeregister.RemoveAll(predicate);
		}

		public void RegisterThing(Thing t)
		{
			thingsToRegister.Add(t);
		}

		public void DeregisterThing(Thing t)
		{
			thingsToDeregister.Add(t);
		}

		public void Tick()
		{
			for (int i = 0; i < thingsToRegister.Count; i++)
			{
				BucketOf(thingsToRegister[i]).Add(thingsToRegister[i]);
			}
			thingsToRegister.Clear();
			for (int j = 0; j < thingsToDeregister.Count; j++)
			{
				BucketOf(thingsToDeregister[j]).Remove(thingsToDeregister[j]);
			}
			thingsToDeregister.Clear();
			if (DebugSettings.fastEcology)
			{
				Find.World.tileTemperatures.ClearCaches();
				for (int k = 0; k < thingLists.Count; k++)
				{
					List<Thing> list = thingLists[k];
					for (int l = 0; l < list.Count; l++)
					{
						if (list[l].def.category == ThingCategory.Plant)
						{
							list[l].TickLong();
						}
					}
				}
			}
			List<Thing> list2 = thingLists[Find.TickManager.TicksGame % TickInterval];
			for (int m = 0; m < list2.Count; m++)
			{
				if (!list2[m].Destroyed)
				{
					try
					{
						switch (tickType)
						{
						case TickerType.Normal:
							list2[m].Tick();
							break;
						case TickerType.Rare:
							list2[m].TickRare();
							break;
						case TickerType.Long:
							list2[m].TickLong();
							break;
						}
					}
					catch (Exception ex)
					{
						string text = list2[m].Spawned ? (" (at " + list2[m].Position + ")") : "";
						if (Prefs.DevMode)
						{
							Log.Error("Exception ticking " + list2[m].ToStringSafe() + text + ": " + ex);
						}
						else
						{
							Log.ErrorOnce("Exception ticking " + list2[m].ToStringSafe() + text + ". Suppressing further errors. Exception: " + ex, list2[m].thingIDNumber ^ 0x22627165);
						}
					}
				}
			}
		}

		private List<Thing> BucketOf(Thing t)
		{
			int num = t.GetHashCode();
			if (num < 0)
			{
				num *= -1;
			}
			int index = num % TickInterval;
			return thingLists[index];
		}
	}
}
