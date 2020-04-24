using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class ListerFilthInHomeArea
	{
		private Map map;

		private List<Thing> filthInHomeArea = new List<Thing>();

		public List<Thing> FilthInHomeArea => filthInHomeArea;

		public ListerFilthInHomeArea(Map map)
		{
			this.map = map;
		}

		public void RebuildAll()
		{
			filthInHomeArea.Clear();
			foreach (IntVec3 allCell in map.AllCells)
			{
				Notify_HomeAreaChanged(allCell);
			}
		}

		public void Notify_FilthSpawned(Filth f)
		{
			if (map.areaManager.Home[f.Position])
			{
				filthInHomeArea.Add(f);
			}
		}

		public void Notify_FilthDespawned(Filth f)
		{
			int num = 0;
			while (true)
			{
				if (num < filthInHomeArea.Count)
				{
					if (filthInHomeArea[num] == f)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			filthInHomeArea.RemoveAt(num);
		}

		public void Notify_HomeAreaChanged(IntVec3 c)
		{
			if (map.areaManager.Home[c])
			{
				List<Thing> thingList = c.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Filth filth = thingList[i] as Filth;
					if (filth != null)
					{
						filthInHomeArea.Add(filth);
					}
				}
				return;
			}
			for (int num = filthInHomeArea.Count - 1; num >= 0; num--)
			{
				if (filthInHomeArea[num].Position == c)
				{
					filthInHomeArea.RemoveAt(num);
				}
			}
		}

		internal string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= Filth in home area");
			foreach (Thing item in filthInHomeArea)
			{
				stringBuilder.AppendLine(item.ThingID + " " + item.Position);
			}
			return stringBuilder.ToString();
		}
	}
}
