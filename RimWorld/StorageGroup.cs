using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StorageGroup : IExposable, ILoadReferenceable, IStoreSettingsParent, ISlotGroup, IRenameable
{
	public int loadID;

	private Map map;

	[LoadAlias("buildings")]
	public List<IStorageGroupMember> members = new List<IStorageGroupMember>();

	private StorageSettings settings;

	private string label;

	private static readonly List<IntVec3> tmpCellsList = new List<IntVec3>(128);

	private static readonly List<IHaulSource> tmpHaulSourceList = new List<IHaulSource>(64);

	public string RenamableLabel
	{
		get
		{
			return label ?? BaseLabel;
		}
		set
		{
			label = value;
		}
	}

	public string BaseLabel => "StorageGroup".Translate();

	public string GroupingLabel => "GroupPlural".Translate();

	public int GroupingOrder => -100;

	public string InspectLabel => RenamableLabel;

	public Map Map => map;

	public int MemberCount => members.Count;

	public bool StorageTabVisible => true;

	StorageSettings ISlotGroup.Settings => GetStoreSettings();

	StorageGroup ISlotGroup.StorageGroup => this;

	public IEnumerable<Thing> HeldThings
	{
		get
		{
			foreach (IStorageGroupMember member in members)
			{
				if (!(member is ISlotGroupParent slotGroupParent))
				{
					continue;
				}
				foreach (Thing heldThing in slotGroupParent.GetSlotGroup().HeldThings)
				{
					yield return heldThing;
				}
			}
		}
	}

	public List<IntVec3> CellsList
	{
		get
		{
			tmpCellsList.Clear();
			foreach (IStorageGroupMember member in members)
			{
				if (member is ISlotGroupParent slotGroupParent)
				{
					tmpCellsList.AddRange(slotGroupParent.GetSlotGroup().CellsList);
				}
			}
			return tmpCellsList;
		}
	}

	public List<IHaulSource> HaulSourcesList
	{
		get
		{
			tmpHaulSourceList.Clear();
			foreach (IStorageGroupMember member in members)
			{
				if (member is IHaulSource item)
				{
					tmpHaulSourceList.Add(item);
				}
			}
			return tmpHaulSourceList;
		}
	}

	public StorageGroup()
	{
	}

	public StorageGroup(Map map, string label = null)
	{
		this.map = map;
		settings = new StorageSettings(this);
		RenamableLabel = label ?? StorageGroupManager.NewStorageName(BaseLabel);
	}

	public void InitFrom(IStorageGroupMember member)
	{
		settings.CopyFrom(member.StoreSettings);
	}

	public void RemoveMember(IStorageGroupMember member, bool removeIfEmpty = true)
	{
		if (members.Remove(member))
		{
			if (member is IStoreSettingsParent storeSettingsParent)
			{
				storeSettingsParent.Notify_SettingsChanged();
			}
			if (removeIfEmpty)
			{
				map.storageGroups.Notify_MemberRemoved(this);
			}
			BillUtility.Notify_ISlotGroupRemoved(this);
		}
	}

	public StorageSettings GetStoreSettings()
	{
		return settings;
	}

	public StorageSettings GetParentStoreSettings()
	{
		if (members.Any())
		{
			return members[0].ParentStoreSettings;
		}
		return null;
	}

	public void Notify_SettingsChanged()
	{
		foreach (IStorageGroupMember member in members)
		{
			if (member is ISlotGroupParent slotGroupParent)
			{
				slotGroupParent.Notify_SettingsChanged();
			}
		}
	}

	public string GetUniqueLoadID()
	{
		return "StorageGroup_" + loadID;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_Values.Look(ref label, "label");
		Scribe_References.Look(ref map, "map");
		Scribe_Collections.Look(ref members, "members", LookMode.Reference);
		Scribe_Deep.Look(ref settings, "settings", this);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			members.RemoveAll((IStorageGroupMember x) => x == null);
		}
	}
}
