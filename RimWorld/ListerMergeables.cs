using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ListerMergeables
	{
		private Map map;

		private List<Thing> mergeables = new List<Thing>();

		private string debugOutput = "uninitialized";

		public ListerMergeables(Map map)
		{
			this.map = map;
		}

		public List<Thing> ThingsPotentiallyNeedingMerging()
		{
			return mergeables;
		}

		public void Notify_Spawned(Thing t)
		{
			CheckAdd(t);
		}

		public void Notify_DeSpawned(Thing t)
		{
			TryRemove(t);
		}

		public void Notify_Unforbidden(Thing t)
		{
			CheckAdd(t);
		}

		public void Notify_Forbidden(Thing t)
		{
			TryRemove(t);
		}

		public void Notify_SlotGroupChanged(SlotGroup sg)
		{
			if (sg.CellsList != null)
			{
				for (int i = 0; i < sg.CellsList.Count; i++)
				{
					RecalcAllInCell(sg.CellsList[i]);
				}
			}
		}

		public void Notify_ThingStackChanged(Thing t)
		{
			Check(t);
		}

		public void RecalcAllInCell(IntVec3 c)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Check(thingList[i]);
			}
		}

		private void Check(Thing t)
		{
			if (ShouldBeMergeable(t))
			{
				if (!mergeables.Contains(t))
				{
					mergeables.Add(t);
				}
			}
			else
			{
				mergeables.Remove(t);
			}
		}

		private bool ShouldBeMergeable(Thing t)
		{
			if (t.IsForbidden(Faction.OfPlayer))
			{
				return false;
			}
			if (t.GetSlotGroup() == null)
			{
				return false;
			}
			if (t.stackCount == t.def.stackLimit)
			{
				return false;
			}
			return true;
		}

		private void CheckAdd(Thing t)
		{
			if (ShouldBeMergeable(t) && !mergeables.Contains(t))
			{
				mergeables.Add(t);
			}
		}

		private void TryRemove(Thing t)
		{
			if (t.def.category == ThingCategory.Item)
			{
				mergeables.Remove(t);
			}
		}

		internal string DebugString()
		{
			if (Time.frameCount % 10 == 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("======= All mergeables (Count " + mergeables.Count + ")");
				int num = 0;
				foreach (Thing mergeable in mergeables)
				{
					stringBuilder.AppendLine(mergeable.ThingID);
					num++;
					if (num > 200)
					{
						break;
					}
				}
				debugOutput = stringBuilder.ToString();
			}
			return debugOutput;
		}
	}
}
