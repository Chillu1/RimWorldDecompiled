using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class SlotGroup : ISlotGroup
{
	public ISlotGroupParent parent;

	private Map Map => parent.Map;

	public StorageSettings Settings => parent.GetStoreSettings();

	public string GroupingLabel => parent.GroupingLabel;

	public int GroupingOrder => parent.GroupingOrder;

	public IEnumerable<Thing> HeldThings
	{
		get
		{
			List<IntVec3> cellsList = CellsList;
			Map map = Map;
			for (int i = 0; i < cellsList.Count; i++)
			{
				List<Thing> thingList = map.thingGrid.ThingsListAt(cellsList[i]);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j].def.EverStorable(willMinifyIfPossible: false))
					{
						yield return thingList[j];
					}
				}
			}
		}
	}

	public int HeldThingsCount
	{
		get
		{
			int num = 0;
			List<IntVec3> cellsList = CellsList;
			ThingGrid thingGrid = Map.thingGrid;
			for (int i = 0; i < cellsList.Count; i++)
			{
				List<Thing> list = thingGrid.ThingsListAt(cellsList[i]);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].def.EverStorable(willMinifyIfPossible: false))
					{
						num++;
					}
				}
			}
			return num;
		}
	}

	public List<IntVec3> CellsList => parent.AllSlotCellsList();

	public StorageGroup StorageGroup
	{
		get
		{
			if (parent is IStorageGroupMember storageGroupMember)
			{
				return storageGroupMember.Group;
			}
			return null;
		}
	}

	public IEnumerator<IntVec3> GetEnumerator()
	{
		List<IntVec3> cellsList = CellsList;
		for (int i = 0; i < cellsList.Count; i++)
		{
			yield return cellsList[i];
		}
	}

	public SlotGroup(ISlotGroupParent parent)
	{
		this.parent = parent;
	}

	public void Notify_AddedCell(IntVec3 c)
	{
		Map.haulDestinationManager.SetCellFor(c, this);
		Map.listerHaulables.RecalcAllInCell(c);
		Map.listerMergeables.RecalcAllInCell(c);
	}

	public void Notify_LostCell(IntVec3 c)
	{
		Map.haulDestinationManager.ClearCellFor(c, this);
		Map.listerHaulables.RecalcAllInCell(c);
		Map.listerMergeables.RecalcAllInCell(c);
	}

	public void RemoveHaulDesignationOnStoredThings()
	{
		if (parent.Map == null)
		{
			return;
		}
		foreach (Thing heldThing in HeldThings)
		{
			if (Settings.AllowedToAccept(heldThing))
			{
				Designation designation = Map.designationManager.DesignationOn(heldThing, DesignationDefOf.Haul);
				if (designation != null)
				{
					Map.designationManager.RemoveDesignation(designation);
				}
			}
		}
	}

	public override string ToString()
	{
		if (parent != null)
		{
			return parent.ToString();
		}
		return "NullParent";
	}

	public string GetName()
	{
		if (parent is Zone_Stockpile zone_Stockpile)
		{
			return zone_Stockpile.label;
		}
		if (parent is Building_Storage building_Storage)
		{
			return building_Storage.Label;
		}
		return "UnresolvedSlotGroupName";
	}

	public static string GetGroupLabel(ISlotGroup group)
	{
		if (group is SlotGroup slotGroup)
		{
			return slotGroup.parent.SlotYielderLabel();
		}
		if (group is IRenameable renameable)
		{
			return renameable.RenamableLabel;
		}
		return "UNABLE TO GET LABEL";
	}
}
