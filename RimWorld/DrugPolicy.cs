using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class DrugPolicy : Policy
{
	public DrugPolicyDef sourceDef;

	private List<DrugPolicyEntry> entriesInt;

	public int Count => entriesInt.Count;

	protected override string LoadKey => "DrugPolicy";

	public DrugPolicyEntry this[int index]
	{
		get
		{
			return entriesInt[index];
		}
		set
		{
			entriesInt[index] = value;
		}
	}

	public DrugPolicyEntry this[ThingDef drugDef]
	{
		get
		{
			for (int i = 0; i < entriesInt.Count; i++)
			{
				if (entriesInt[i].drug == drugDef)
				{
					return entriesInt[i];
				}
			}
			throw new ArgumentException();
		}
	}

	public DrugPolicy()
	{
	}

	public DrugPolicy(int id, string label)
		: base(id, label)
	{
		InitializeIfNeeded();
	}

	private void InitializeIfNeeded(bool overwriteExisting = true)
	{
		if (overwriteExisting)
		{
			if (entriesInt != null)
			{
				return;
			}
			entriesInt = new List<DrugPolicyEntry>();
		}
		List<ThingDef> thingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
		int i;
		for (i = 0; i < thingDefs.Count; i++)
		{
			if (thingDefs[i].category == ThingCategory.Item && thingDefs[i].IsDrug && (overwriteExisting || !entriesInt.Any((DrugPolicyEntry x) => x.drug == thingDefs[i])))
			{
				entriesInt.Add(new DrugPolicyEntry
				{
					drug = thingDefs[i],
					allowedForAddiction = true
				});
			}
		}
		entriesInt.SortBy((DrugPolicyEntry e) => e.drug.GetCompProperties<CompProperties_Drug>().listOrder);
	}

	public override void CopyFrom(Policy other)
	{
		if (!(other is DrugPolicy drugPolicy))
		{
			return;
		}
		sourceDef = drugPolicy.sourceDef;
		entriesInt.Clear();
		foreach (DrugPolicyEntry item in drugPolicy.entriesInt)
		{
			DrugPolicyEntry drugPolicyEntry = new DrugPolicyEntry();
			drugPolicyEntry.CopyFrom(item);
			entriesInt.Add(drugPolicyEntry);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref entriesInt, "drugs", LookMode.Deep);
		Scribe_Defs.Look(ref sourceDef, "sourceDef");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && entriesInt != null)
		{
			if (entriesInt.RemoveAll((DrugPolicyEntry x) => x == null || x.drug == null) != 0)
			{
				Log.Error("Some DrugPolicyEntries were null after loading.");
			}
			InitializeIfNeeded(overwriteExisting: false);
		}
	}
}
