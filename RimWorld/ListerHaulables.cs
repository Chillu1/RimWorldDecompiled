using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ListerHaulables
	{
		private Map map;

		private List<Thing> haulables = new List<Thing>();

		private const int CellsPerTick = 4;

		private static int groupCycleIndex;

		private List<int> cellCycleIndices = new List<int>();

		private string debugOutput = "uninitialized";

		public ListerHaulables(Map map)
		{
			this.map = map;
		}

		public List<Thing> ThingsPotentiallyNeedingHauling()
		{
			return haulables;
		}

		public void Notify_Spawned(Thing t)
		{
			CheckAdd(t);
		}

		public void Notify_DeSpawned(Thing t)
		{
			TryRemove(t);
		}

		public void HaulDesignationAdded(Thing t)
		{
			CheckAdd(t);
		}

		public void HaulDesignationRemoved(Thing t)
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
			List<IntVec3> cellsList = sg.CellsList;
			if (cellsList != null)
			{
				for (int i = 0; i < cellsList.Count; i++)
				{
					RecalcAllInCell(cellsList[i]);
				}
			}
		}

		public void ListerHaulablesTick()
		{
			groupCycleIndex++;
			if (groupCycleIndex >= 2147473647)
			{
				groupCycleIndex = 0;
			}
			List<SlotGroup> allGroupsListForReading = map.haulDestinationManager.AllGroupsListForReading;
			if (allGroupsListForReading.Count == 0)
			{
				return;
			}
			int num = groupCycleIndex % allGroupsListForReading.Count;
			SlotGroup slotGroup = allGroupsListForReading[groupCycleIndex % allGroupsListForReading.Count];
			if (slotGroup.CellsList.Count == 0)
			{
				return;
			}
			while (cellCycleIndices.Count <= num)
			{
				cellCycleIndices.Add(0);
			}
			if (cellCycleIndices[num] >= 2147473647)
			{
				cellCycleIndices[num] = 0;
			}
			for (int i = 0; i < 4; i++)
			{
				cellCycleIndices[num]++;
				List<Thing> thingList = slotGroup.CellsList[cellCycleIndices[num] % slotGroup.CellsList.Count].GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j].def.EverHaulable)
					{
						Check(thingList[j]);
						break;
					}
				}
			}
		}

		public void RecalcAllInCell(IntVec3 c)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Check(thingList[i]);
			}
		}

		public void RecalcAllInCells(IEnumerable<IntVec3> cells)
		{
			foreach (IntVec3 cell in cells)
			{
				RecalcAllInCell(cell);
			}
		}

		private void Check(Thing t)
		{
			if (ShouldBeHaulable(t))
			{
				if (!haulables.Contains(t))
				{
					haulables.Add(t);
				}
			}
			else if (haulables.Contains(t))
			{
				haulables.Remove(t);
			}
		}

		private bool ShouldBeHaulable(Thing t)
		{
			if (t.IsForbidden(Faction.OfPlayer))
			{
				return false;
			}
			if (!t.def.alwaysHaulable)
			{
				if (!t.def.EverHaulable)
				{
					return false;
				}
				if (map.designationManager.DesignationOn(t, DesignationDefOf.Haul) == null && !t.IsInAnyStorage())
				{
					return false;
				}
			}
			if (t.IsInValidBestStorage())
			{
				return false;
			}
			return true;
		}

		private void CheckAdd(Thing t)
		{
			if (ShouldBeHaulable(t) && !haulables.Contains(t))
			{
				haulables.Add(t);
			}
		}

		private void TryRemove(Thing t)
		{
			if (t.def.category == ThingCategory.Item && haulables.Contains(t))
			{
				haulables.Remove(t);
			}
		}

		internal string DebugString()
		{
			if (Time.frameCount % 10 == 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("======= All haulables (Count " + haulables.Count + ")");
				int num = 0;
				foreach (Thing haulable in haulables)
				{
					stringBuilder.AppendLine(haulable.ThingID);
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
