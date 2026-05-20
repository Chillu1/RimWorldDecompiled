using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class StorageGroupManager : IExposable
{
	public Map map;

	private List<StorageGroup> groups = new List<StorageGroup>();

	public List<StorageGroup> StorageGroupsForReading => groups;

	public StorageGroupManager(Map map)
	{
		this.map = map;
	}

	public StorageGroup NewGroup(string label = null)
	{
		StorageGroup storageGroup = new StorageGroup(map, label);
		storageGroup.loadID = Find.UniqueIDsManager.GetNextStorageGroupID();
		groups.Add(storageGroup);
		return storageGroup;
	}

	public void Notify_MemberRemoved(StorageGroup group)
	{
		if (group.MemberCount <= 1)
		{
			for (int num = group.MemberCount - 1; num >= 0; num--)
			{
				group.members[num].SetStorageGroup(null);
			}
			groups.Remove(group);
		}
	}

	public static string NewStorageName(string nameBase)
	{
		for (int i = 1; i <= 1000; i++)
		{
			string cand = nameBase + " " + i;
			if (!Current.Game.CurrentMap.zoneManager.AllZones.Any((Zone z) => z.label == cand) && !Current.Game.CurrentMap.haulDestinationManager.AllGroups.Any((SlotGroup x) => x.parent is Building_Storage building_Storage && building_Storage.label == cand) && !Current.Game.CurrentMap.storageGroups.groups.Any((StorageGroup g) => g.RenamableLabel == cand))
			{
				return cand;
			}
		}
		Log.Error("Ran out of storage " + nameBase + " names.");
		return nameBase + " X";
	}

	public bool HasGroupWithName(string label)
	{
		return groups.Any((StorageGroup x) => x.RenamableLabel == label);
	}

	public bool HasStorageGroup(StorageGroup group)
	{
		return groups.Contains(group);
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref groups, "groups", LookMode.Deep);
		Scribe_References.Look(ref map, "map");
	}
}
