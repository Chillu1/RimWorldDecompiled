using RimWorld;

namespace Verse;

public class Dialog_RenameBuildingStorage_CreateNew : Dialog_Rename<IRenameable>
{
	private IStorageGroupMember building;

	public Dialog_RenameBuildingStorage_CreateNew(IStorageGroupMember building)
		: base((IRenameable)null)
	{
		this.building = building;
		curName = building.Group?.RenamableLabel ?? string.Empty;
	}

	protected override AcceptanceReport NameIsValid(string name)
	{
		AcceptanceReport result = base.NameIsValid(name);
		if (!result.Accepted)
		{
			return result;
		}
		if (Current.Game.CurrentMap.zoneManager.AllZones.Any((Zone z) => z.label == name) || Current.Game.CurrentMap.storageGroups.HasGroupWithName(name))
		{
			return "NameIsInUse".Translate();
		}
		return true;
	}

	protected override void OnRenamed(string name)
	{
		StorageGroup storageGroup = building.Map.storageGroups.NewGroup();
		storageGroup.RenamableLabel = name;
		storageGroup.InitFrom(building);
		building.SetStorageGroup(storageGroup);
	}
}
