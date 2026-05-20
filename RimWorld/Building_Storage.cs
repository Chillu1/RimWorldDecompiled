using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Building_Storage : Building, ISlotGroupParent, IStoreSettingsParent, IHaulDestination, IStorageGroupMember, IHaulEnroute, ILoadReferenceable
{
	public StorageSettings settings;

	public StorageGroup storageGroup;

	public string label;

	public SlotGroup slotGroup;

	private List<IntVec3> cachedOccupiedCells;

	private static StringBuilder sb = new StringBuilder();

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

	bool IStorageGroupMember.DrawConnectionOverlay => base.Spawned;

	Map IStorageGroupMember.Map => base.MapHeld;

	string IStorageGroupMember.StorageGroupTag => def.building.storageGroupTag;

	StorageSettings IStorageGroupMember.StoreSettings => GetStoreSettings();

	StorageSettings IStorageGroupMember.ParentStoreSettings => GetParentStoreSettings();

	StorageSettings IStorageGroupMember.ThingStoreSettings => settings;

	bool IStorageGroupMember.DrawStorageTab => true;

	bool IStorageGroupMember.ShowRenameButton => base.Faction == Faction.OfPlayer;

	public bool StorageTabVisible => true;

	public bool IgnoreStoredThingsBeauty => def.building.ignoreStoredThingsBeauty;

	public string GroupingLabel => def.building.groupingLabel;

	public int GroupingOrder => def.building.groupingOrder;

	public bool HaulDestinationEnabled => true;

	public Building_Storage()
	{
		slotGroup = new SlotGroup(this);
	}

	public SlotGroup GetSlotGroup()
	{
		return slotGroup;
	}

	public virtual void Notify_ReceivedThing(Thing newItem)
	{
		if (base.Faction == Faction.OfPlayer && newItem.def.storedConceptLearnOpportunity != null)
		{
			LessonAutoActivator.TeachOpportunity(newItem.def.storedConceptLearnOpportunity, OpportunityType.GoodToKnow);
		}
	}

	public virtual void Notify_LostThing(Thing newItem)
	{
	}

	public virtual IEnumerable<IntVec3> AllSlotCells()
	{
		if (!base.Spawned)
		{
			yield break;
		}
		foreach (IntVec3 item in GenAdj.CellsOccupiedBy(this))
		{
			yield return item;
		}
	}

	public List<IntVec3> AllSlotCellsList()
	{
		return cachedOccupiedCells ?? (cachedOccupiedCells = AllSlotCells().ToList());
	}

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
		StorageSettings fixedStorageSettings = def.building.fixedStorageSettings;
		if (fixedStorageSettings != null)
		{
			return fixedStorageSettings;
		}
		return StorageSettings.EverStorableFixedSettings();
	}

	public void Notify_SettingsChanged()
	{
		if (base.Spawned && slotGroup != null)
		{
			base.Map.listerHaulables.Notify_SlotGroupChanged(slotGroup);
		}
	}

	public string SlotYielderLabel()
	{
		return LabelCap;
	}

	public bool Accepts(Thing t)
	{
		return GetStoreSettings().AllowedToAccept(t);
	}

	public int SpaceRemainingFor(ThingDef _)
	{
		return slotGroup.HeldThingsCount - def.building.maxItemsInCell * def.Size.Area;
	}

	public override void PostMake()
	{
		base.PostMake();
		settings = new StorageSettings(this);
		if (def.building.defaultStorageSettings != null)
		{
			settings.CopyFrom(def.building.defaultStorageSettings);
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		cachedOccupiedCells = null;
		base.SpawnSetup(map, respawningAfterLoad);
		if (storageGroup != null && map != storageGroup.Map)
		{
			StorageSettings storeSettings = storageGroup.GetStoreSettings();
			storageGroup.RemoveMember(this);
			storageGroup = null;
			settings.CopyFrom(storeSettings);
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.DeSpawn(mode);
		cachedOccupiedCells = null;
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		base.Destroy(mode);
		if (storageGroup != null)
		{
			storageGroup?.RemoveMember(this);
			storageGroup = null;
		}
		BillUtility.Notify_ISlotGroupRemoved(slotGroup);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref settings, "settings", this);
		Scribe_References.Look(ref storageGroup, "storageGroup");
		Scribe_Values.Look(ref label, "label");
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		StorageGroupUtility.DrawSelectionOverlaysFor(this);
	}

	public override string GetInspectString()
	{
		sb.Clear();
		sb.Append(base.GetInspectString());
		if (base.Spawned)
		{
			if (storageGroup != null)
			{
				sb.AppendLineIfNotEmpty();
				sb.Append(string.Format("{0}: {1} ", "StorageGroupLabel".Translate(), storageGroup.RenamableLabel.CapitalizeFirst()));
				if (storageGroup.MemberCount > 1)
				{
					sb.Append(string.Format("({0})", "NumBuildings".Translate(storageGroup.MemberCount)));
				}
				else
				{
					sb.Append(string.Format("({0})", "OneBuilding".Translate()));
				}
			}
			if (slotGroup.HeldThings.Any())
			{
				sb.AppendLineIfNotEmpty();
				sb.Append("StoresThings".Translate());
				sb.Append(": ");
				sb.Append(slotGroup.HeldThings.Select((Thing x) => x.LabelShortCap).Distinct().ToCommaList());
				sb.Append(".");
			}
		}
		return sb.ToString();
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
		if (!StorageTabVisible || base.MapHeld == null)
		{
			yield break;
		}
		foreach (Gizmo item2 in StorageGroupUtility.StorageGroupMemberGizmos(this))
		{
			yield return item2;
		}
		if (Find.Selector.NumSelected != 1)
		{
			yield break;
		}
		foreach (Thing heldThing in slotGroup.HeldThings)
		{
			yield return ContainingSelectionUtility.CreateSelectStorageGizmo("CommandSelectStoredThing".Translate(heldThing), ("CommandSelectStoredThingDesc".Translate() + "\n\n" + heldThing.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + heldThing.GetInspectString()).Resolve(), heldThing, heldThing, groupable: false);
		}
	}
}
