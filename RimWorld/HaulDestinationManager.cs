using System.Collections.Generic;
using Verse;

namespace RimWorld;

public sealed class HaulDestinationManager
{
	private Map map;

	private readonly List<IHaulDestination> allHaulDestinationsInOrder = new List<IHaulDestination>();

	private readonly List<IHaulSource> allHaulSourcesInOrder = new List<IHaulSource>();

	private readonly List<SlotGroup> allGroupsInOrder = new List<SlotGroup>();

	private readonly SlotGroup[,,] groupGrid;

	public IEnumerable<IHaulDestination> AllHaulDestinations => allHaulDestinationsInOrder;

	public List<IHaulDestination> AllHaulDestinationsListForReading => allHaulDestinationsInOrder;

	public List<IHaulDestination> AllHaulDestinationsListInPriorityOrder => allHaulDestinationsInOrder;

	public List<IHaulSource> AllHaulSourcesListInPriorityOrder => allHaulSourcesInOrder;

	public IEnumerable<SlotGroup> AllGroups => allGroupsInOrder;

	public List<SlotGroup> AllGroupsListForReading => allGroupsInOrder;

	public List<IHaulSource> AllHaulSourcesListForReading => allHaulSourcesInOrder;

	public List<SlotGroup> AllGroupsListInPriorityOrder => allGroupsInOrder;

	public IEnumerable<IntVec3> AllSlots
	{
		get
		{
			for (int i = 0; i < allGroupsInOrder.Count; i++)
			{
				List<IntVec3> cellsList = allGroupsInOrder[i].CellsList;
				int j = 0;
				while (j < allGroupsInOrder.Count)
				{
					yield return cellsList[j];
					i++;
				}
			}
		}
	}

	public HaulDestinationManager(Map map)
	{
		this.map = map;
		groupGrid = new SlotGroup[map.Size.x, map.Size.y, map.Size.z];
	}

	public void AddHaulDestination(IHaulDestination haulDestination)
	{
		if (allHaulDestinationsInOrder.Contains(haulDestination))
		{
			Log.Error("Double-added haul destination " + haulDestination.ToStringSafe());
			return;
		}
		allHaulDestinationsInOrder.Add(haulDestination);
		allHaulDestinationsInOrder.InsertionSort(CompareHaulDestinationPrioritiesDescending);
		if (!(haulDestination is ISlotGroupParent slotGroupParent))
		{
			return;
		}
		SlotGroup slotGroup = slotGroupParent.GetSlotGroup();
		if (slotGroup == null)
		{
			Log.Error("ISlotGroupParent gave null slot group: " + slotGroupParent.ToStringSafe());
			return;
		}
		allGroupsInOrder.Add(slotGroup);
		allGroupsInOrder.InsertionSort(CompareSlotGroupPrioritiesDescending);
		List<IntVec3> cellsList = slotGroup.CellsList;
		for (int i = 0; i < cellsList.Count; i++)
		{
			SetCellFor(cellsList[i], slotGroup);
		}
		map.listerHaulables.Notify_SlotGroupChanged(slotGroup);
		map.listerMergeables.Notify_SlotGroupChanged(slotGroup);
	}

	public void RemoveHaulDestination(IHaulDestination haulDestination)
	{
		if (!allHaulDestinationsInOrder.Contains(haulDestination))
		{
			Log.Error("Removing haul destination that isn't registered " + haulDestination.ToStringSafe());
			return;
		}
		allHaulDestinationsInOrder.Remove(haulDestination);
		if (!(haulDestination is ISlotGroupParent slotGroupParent))
		{
			return;
		}
		SlotGroup slotGroup = slotGroupParent.GetSlotGroup();
		if (slotGroup == null)
		{
			Log.Error("ISlotGroupParent gave null slot group: " + slotGroupParent.ToStringSafe());
			return;
		}
		allGroupsInOrder.Remove(slotGroup);
		List<IntVec3> cellsList = slotGroup.CellsList;
		for (int i = 0; i < cellsList.Count; i++)
		{
			IntVec3 intVec = cellsList[i];
			groupGrid[intVec.x, intVec.y, intVec.z] = null;
		}
		map.listerHaulables.Notify_SlotGroupChanged(slotGroup);
		map.listerMergeables.Notify_SlotGroupChanged(slotGroup);
	}

	public void AddHaulSource(IHaulSource source)
	{
		if (allHaulSourcesInOrder.Contains(source))
		{
			Log.Error("Double-added haul destination " + source.ToStringSafe());
			return;
		}
		allHaulSourcesInOrder.Add(source);
		allHaulSourcesInOrder.InsertionSort(CompareHaulSourcePrioritiesDescending);
		map.listerHaulables.Notify_HaulSourceChanged(source);
	}

	public void RemoveHaulSource(IHaulSource source)
	{
		if (!allHaulSourcesInOrder.Remove(source))
		{
			Log.Error("Removing haul source that isn't registered " + source.ToStringSafe());
		}
	}

	public void Notify_HaulDestinationChangedPriority()
	{
		allHaulDestinationsInOrder.InsertionSort(CompareHaulDestinationPrioritiesDescending);
		allGroupsInOrder.InsertionSort(CompareSlotGroupPrioritiesDescending);
		allHaulSourcesInOrder.InsertionSort(CompareHaulSourcePrioritiesDescending);
	}

	private static int CompareHaulDestinationPrioritiesDescending(IHaulDestination a, IHaulDestination b)
	{
		return ((int)b.GetStoreSettings().Priority).CompareTo((int)a.GetStoreSettings().Priority);
	}

	private static int CompareHaulSourcePrioritiesDescending(IHaulSource a, IHaulSource b)
	{
		return ((int)b.GetStoreSettings().Priority).CompareTo((int)a.GetStoreSettings().Priority);
	}

	private static int CompareSlotGroupPrioritiesDescending(SlotGroup a, SlotGroup b)
	{
		return ((int)b.Settings.Priority).CompareTo((int)a.Settings.Priority);
	}

	public SlotGroup SlotGroupAt(IntVec3 loc)
	{
		return groupGrid[loc.x, loc.y, loc.z];
	}

	public ISlotGroupParent SlotGroupParentAt(IntVec3 loc)
	{
		return SlotGroupAt(loc)?.parent;
	}

	public void SetCellFor(IntVec3 c, SlotGroup group)
	{
		if (SlotGroupAt(c) != null)
		{
			string[] obj = new string[5]
			{
				group?.ToString(),
				" overwriting slot group square ",
				null,
				null,
				null
			};
			IntVec3 intVec = c;
			obj[2] = intVec.ToString();
			obj[3] = " of ";
			obj[4] = SlotGroupAt(c)?.ToString();
			Log.Error(string.Concat(obj));
		}
		groupGrid[c.x, c.y, c.z] = group;
	}

	public void ClearCellFor(IntVec3 c, SlotGroup group)
	{
		if (SlotGroupAt(c) != group)
		{
			string[] obj = new string[5]
			{
				group?.ToString(),
				" clearing group grid square ",
				null,
				null,
				null
			};
			IntVec3 intVec = c;
			obj[2] = intVec.ToString();
			obj[3] = " containing ";
			obj[4] = SlotGroupAt(c)?.ToString();
			Log.Error(string.Concat(obj));
		}
		groupGrid[c.x, c.y, c.z] = null;
	}
}
