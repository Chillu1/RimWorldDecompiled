using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ListerHaulables
{
	private Map map;

	private HashSet<Thing> haulables = new HashSet<Thing>();

	private const int CellsPerTick = 4;

	private const int HaulSourcesPerTick = 4;

	private static int groupCycleIndex = 0;

	private static readonly List<int> cellCycleIndices = new List<int>();

	private string debugOutput = "uninitialized";

	public ListerHaulables(Map map)
	{
		this.map = map;
	}

	public ICollection<Thing> ThingsPotentiallyNeedingHauling()
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

	public void Notify_AddedThing(Thing t)
	{
		CheckAdd(t);
	}

	public void Notify_SlotGroupChanged(SlotGroup sg)
	{
		List<IntVec3> cellsList = sg.CellsList;
		if (cellsList != null)
		{
			sg.RemoveHaulDesignationOnStoredThings();
			for (int i = 0; i < cellsList.Count; i++)
			{
				RecalcAllInCell(cellsList[i]);
			}
		}
	}

	public void Notify_HaulSourceChanged(IHaulSource holder)
	{
		foreach (Thing item in (IEnumerable<Thing>)holder.GetDirectlyHeldThings())
		{
			Check(item);
		}
	}

	public void ListerHaulablesTick()
	{
		groupCycleIndex++;
		if (groupCycleIndex >= 2147473647)
		{
			groupCycleIndex = 0;
		}
		CellsCheckTick(map.haulDestinationManager.AllGroupsListForReading);
		HaulSourcesCheckTick(map.haulDestinationManager.AllHaulSourcesListForReading);
	}

	private void CellsCheckTick(List<SlotGroup> sgList)
	{
		if (sgList.Count == 0)
		{
			return;
		}
		int num = groupCycleIndex % sgList.Count;
		SlotGroup slotGroup = sgList[groupCycleIndex % sgList.Count];
		if (slotGroup.CellsList.Count != 0)
		{
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
				RecalcAllInCell(slotGroup.CellsList[cellCycleIndices[num] % slotGroup.CellsList.Count]);
			}
		}
	}

	private void HaulSourcesCheckTick(List<IHaulSource> haulList)
	{
		if (haulList.Count == 0)
		{
			return;
		}
		int num = Mathf.CeilToInt((float)haulList.Count / 4f);
		int num2 = groupCycleIndex % num;
		for (int i = 0; i < 4 && i < haulList.Count; i++)
		{
			int index = num2 + i;
			IHaulSource haulSource = haulList[index];
			if (haulSource.GetDirectlyHeldThings().Count != 0)
			{
				RecalculateAllInHaulSource(haulSource);
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

	public void RecalculateAllInHaulSources(IList<IHaulSource> sources)
	{
		foreach (IHaulSource source in sources)
		{
			foreach (Thing item in (IEnumerable<Thing>)source.GetDirectlyHeldThings())
			{
				Check(item);
			}
		}
	}

	public void RecalculateAllInHaulSource(IHaulSource source)
	{
		foreach (Thing item in (IEnumerable<Thing>)source.GetDirectlyHeldThings())
		{
			Check(item);
		}
	}

	private void Check(Thing t)
	{
		if (ShouldBeHaulable(t))
		{
			haulables.Add(t);
		}
		else
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
		if (t.ParentHolder is IHaulSource { HaulSourceEnabled: false })
		{
			return false;
		}
		return true;
	}

	private void CheckAdd(Thing t)
	{
		if (ShouldBeHaulable(t))
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
