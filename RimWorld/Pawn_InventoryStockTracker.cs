using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Pawn_InventoryStockTracker : IExposable
{
	public Pawn pawn;

	public Dictionary<InventoryStockGroupDef, InventoryStockEntry> stockEntries = new Dictionary<InventoryStockGroupDef, InventoryStockEntry>();

	private List<InventoryStockGroupDef> tmpInventoryStockGroups;

	private List<InventoryStockEntry> tmpInventoryStock;

	public Pawn_InventoryStockTracker()
	{
	}

	public Pawn_InventoryStockTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public InventoryStockEntry GetCurrentEntryFor(InventoryStockGroupDef group)
	{
		if (!stockEntries.ContainsKey(group))
		{
			stockEntries[group] = CreateDefaultEntryFor(group);
		}
		return stockEntries[group];
	}

	public int GetDesiredCountForGroup(InventoryStockGroupDef group)
	{
		return GetCurrentEntryFor(group).count;
	}

	public ThingDef GetDesiredThingForGroup(InventoryStockGroupDef group)
	{
		return GetCurrentEntryFor(group).thingDef;
	}

	public void SetCountForGroup(InventoryStockGroupDef group, int count)
	{
		InventoryStockEntry currentEntryFor = GetCurrentEntryFor(group);
		int count2 = Mathf.Clamp(count, group.min, group.max);
		currentEntryFor.count = count2;
	}

	public void SetThingForGroup(InventoryStockGroupDef group, ThingDef thing)
	{
		if (!group.thingDefs.Contains(thing))
		{
			Log.Error("Inventory stock group " + group.defName + " does not contain " + thing.defName + ".");
		}
		else
		{
			GetCurrentEntryFor(group).thingDef = thing;
		}
	}

	private InventoryStockEntry CreateDefaultEntryFor(InventoryStockGroupDef group)
	{
		return new InventoryStockEntry
		{
			count = group.min,
			thingDef = group.DefaultThingDef
		};
	}

	public bool AnyThingsRequiredNow()
	{
		foreach (KeyValuePair<InventoryStockGroupDef, InventoryStockEntry> stockEntry in stockEntries)
		{
			if (pawn.inventory.Count(stockEntry.Value.thingDef) < stockEntry.Value.count)
			{
				return true;
			}
		}
		return false;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref stockEntries, "stockEntries", LookMode.Def, LookMode.Deep, ref tmpInventoryStockGroups, ref tmpInventoryStock);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		foreach (InventoryStockGroupDef allDef in DefDatabase<InventoryStockGroupDef>.AllDefs)
		{
			InventoryStockEntry currentEntryFor = GetCurrentEntryFor(allDef);
			if (!allDef.thingDefs.Contains(currentEntryFor.thingDef))
			{
				currentEntryFor.thingDef = allDef.DefaultThingDef;
			}
		}
	}
}
