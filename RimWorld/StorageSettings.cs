using System;
using Verse;

namespace RimWorld;

public class StorageSettings : IExposable
{
	private static StorageSettings cachedEverStorableFixedSettings;

	public IStoreSettingsParent owner;

	public ThingFilter filter;

	[LoadAlias("priority")]
	private StoragePriority priorityInt = StoragePriority.Normal;

	private IHaulDestination HaulDestinationOwner => owner as IHaulDestination;

	private IHaulSource HaulSourceOwner => owner as IHaulSource;

	private ISlotGroupParent SlotGroupParentOwner => owner as ISlotGroupParent;

	public StoragePriority Priority
	{
		get
		{
			return priorityInt;
		}
		set
		{
			priorityInt = value;
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (owner is StorageGroup storageGroup)
			{
				storageGroup.Map?.haulDestinationManager.Notify_HaulDestinationChangedPriority();
				storageGroup.Map?.listerHaulables.RecalcAllInCells(storageGroup.CellsList);
				storageGroup.Map?.listerHaulables.RecalculateAllInHaulSources(storageGroup.HaulSourcesList);
				return;
			}
			if (HaulDestinationOwner != null && HaulDestinationOwner.Map != null)
			{
				HaulDestinationOwner.Map.haulDestinationManager.Notify_HaulDestinationChangedPriority();
			}
			if (HaulSourceOwner != null && HaulSourceOwner.Map != null)
			{
				HaulSourceOwner.Map.haulDestinationManager.Notify_HaulDestinationChangedPriority();
				HaulSourceOwner.Map.listerHaulables.RecalculateAllInHaulSource(HaulSourceOwner);
			}
			if (SlotGroupParentOwner != null && SlotGroupParentOwner.Map != null)
			{
				SlotGroupParentOwner.Map.listerHaulables.RecalcAllInCells(SlotGroupParentOwner.AllSlotCells());
			}
		}
	}

	public StorageSettings()
	{
		filter = new ThingFilter(TryNotifyChanged);
	}

	public StorageSettings(IStoreSettingsParent owner)
		: this()
	{
		this.owner = owner;
		if (owner != null)
		{
			StorageSettings parentStoreSettings = owner.GetParentStoreSettings();
			if (parentStoreSettings != null)
			{
				priorityInt = parentStoreSettings.priorityInt;
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref priorityInt, "priority", StoragePriority.Unstored);
		Scribe_Deep.Look(ref filter, "filter", new Action(TryNotifyChanged));
	}

	public void SetFromPreset(StorageSettingsPreset preset)
	{
		filter.SetFromPreset(preset);
		TryNotifyChanged();
	}

	public void CopyFrom(StorageSettings other)
	{
		Priority = other.Priority;
		filter.CopyAllowancesFrom(other.filter);
		TryNotifyChanged();
	}

	public bool AllowedToAccept(Thing t)
	{
		if (!filter.Allows(t))
		{
			return false;
		}
		if (owner != null)
		{
			StorageSettings parentStoreSettings = owner.GetParentStoreSettings();
			if (parentStoreSettings != null && !parentStoreSettings.AllowedToAccept(t))
			{
				return false;
			}
		}
		return true;
	}

	public bool AllowedToAccept(ThingDef t)
	{
		if (!filter.Allows(t))
		{
			return false;
		}
		if (owner != null)
		{
			StorageSettings parentStoreSettings = owner.GetParentStoreSettings();
			if (parentStoreSettings != null && !parentStoreSettings.AllowedToAccept(t))
			{
				return false;
			}
		}
		return true;
	}

	private void TryNotifyChanged()
	{
		owner?.Notify_SettingsChanged();
	}

	public static StorageSettings EverStorableFixedSettings()
	{
		if (cachedEverStorableFixedSettings == null)
		{
			cachedEverStorableFixedSettings = new StorageSettings(null)
			{
				filter = ThingFilter.CreateOnlyEverStorableThingFilter()
			};
		}
		return cachedEverStorableFixedSettings;
	}

	public static void ResetStaticData()
	{
		cachedEverStorableFixedSettings = null;
	}
}
