using Verse;

namespace RimWorld;

public interface IStorageGroupMember
{
	StorageGroup Group { get; set; }

	Map Map { get; }

	StorageSettings StoreSettings { get; }

	StorageSettings ParentStoreSettings { get; }

	StorageSettings ThingStoreSettings { get; }

	string StorageGroupTag { get; }

	bool DrawConnectionOverlay { get; }

	bool DrawStorageTab { get; }

	bool ShowRenameButton { get; }
}
