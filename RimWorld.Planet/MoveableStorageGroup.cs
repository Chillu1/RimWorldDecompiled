using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class MoveableStorageGroup : IExposable
{
	public List<IStorageGroupMember> members = new List<IStorageGroupMember>();

	private string label;

	private StorageSettings settings;

	public MoveableStorageGroup()
	{
	}

	public MoveableStorageGroup(StorageGroup storageGroup)
	{
		label = storageGroup.RenamableLabel;
		settings = new StorageSettings();
		settings.CopyFrom(storageGroup.GetStoreSettings());
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref members, "members", LookMode.Reference);
		Scribe_Values.Look(ref label, "label");
		Scribe_Deep.Look(ref settings, "settings");
	}

	public void TryCreateStorageGroup(Map map)
	{
		StorageGroup storageGroup = map.storageGroups.NewGroup(label);
		storageGroup.GetStoreSettings().CopyFrom(settings);
		foreach (IStorageGroupMember member in members)
		{
			member.SetStorageGroup(storageGroup);
		}
	}
}
