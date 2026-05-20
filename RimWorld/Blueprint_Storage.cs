using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Blueprint_Storage : Blueprint_Build, IStorageGroupMember, IStoreSettingsParent
{
	protected StorageGroup storageGroup;

	public StorageSettings settings;

	StorageGroup IStorageGroupMember.Group
	{
		get
		{
			return storageGroup;
		}
		set
		{
			storageGroup = value;
		}
	}

	Map IStorageGroupMember.Map => base.Map;

	StorageSettings IStorageGroupMember.StoreSettings => GetStoreSettings();

	StorageSettings IStorageGroupMember.ParentStoreSettings => GetParentStoreSettings();

	StorageSettings IStorageGroupMember.ThingStoreSettings => settings;

	string IStorageGroupMember.StorageGroupTag => base.BuildDef.building.storageGroupTag;

	bool IStorageGroupMember.DrawConnectionOverlay => true;

	bool IStorageGroupMember.DrawStorageTab => true;

	bool IStorageGroupMember.ShowRenameButton => base.Faction == Faction.OfPlayer;

	bool IStoreSettingsParent.StorageTabVisible => true;

	public StorageSettings GetStoreSettings()
	{
		if (storageGroup != null)
		{
			return storageGroup.GetStoreSettings();
		}
		return settings;
	}

	public StorageSettings GetParentStoreSettings()
	{
		return base.BuildDef.building.fixedStorageSettings;
	}

	void IStoreSettingsParent.Notify_SettingsChanged()
	{
	}

	public override void PostMake()
	{
		base.PostMake();
		settings = new StorageSettings(this);
		if (base.BuildDef.building.defaultStorageSettings != null)
		{
			settings.CopyFrom(base.BuildDef.building.defaultStorageSettings);
		}
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		storageGroup?.RemoveMember(this);
		storageGroup = null;
		base.Destroy(mode);
	}

	public override IEnumerable<InspectTabBase> GetInspectTabs()
	{
		if (base.BuildDef.inspectorTabsResolved == null)
		{
			yield break;
		}
		foreach (InspectTabBase item in base.BuildDef.inspectorTabsResolved)
		{
			if (item.VisibleInBlueprintMode)
			{
				yield return item;
			}
		}
	}

	protected override Thing MakeSolidThing(out bool shouldSelect)
	{
		Frame obj = (Frame)base.MakeSolidThing(out shouldSelect);
		obj.SetStorageGroup(storageGroup);
		obj.storageSettings = new StorageSettings();
		obj.storageSettings.CopyFrom(GetStoreSettings());
		this.SetStorageGroup(null, removeIfEmpty: false);
		return obj;
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		StorageGroupUtility.DrawSelectionOverlaysFor(this);
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (storageGroup != null)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text += "LinkedStorageSettings".Translate() + ": ";
			text = ((storageGroup.MemberCount <= 1) ? ((string)(text + "OneBuilding".Translate().CapitalizeFirst())) : ((string)(text + "NumBuildings".Translate(storageGroup.MemberCount).CapitalizeFirst())));
		}
		return text;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(GetStoreSettings()))
		{
			yield return item;
		}
		foreach (Gizmo item2 in StorageGroupUtility.StorageGroupMemberGizmos(this))
		{
			yield return item2;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref storageGroup, "storageGroup");
		Scribe_Deep.Look(ref settings, "settings", this);
	}
}
