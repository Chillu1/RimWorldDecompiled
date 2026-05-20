using System.Collections.Generic;
using Verse;

namespace RimWorld;

public sealed class HiddenItemsManager : IExposable
{
	private Dictionary<ThingDef, bool> hiddenItemDefs;

	public void SetDiscovered(ThingDef def)
	{
		if (hiddenItemDefs.ContainsKey(def))
		{
			hiddenItemDefs[def] = false;
		}
	}

	public bool Hidden(ThingDef def)
	{
		bool value;
		return hiddenItemDefs.TryGetValue(def, out value) && value;
	}

	public HiddenItemsManager()
	{
		hiddenItemDefs = new Dictionary<ThingDef, bool>();
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef.hiddenWhileUndiscovered)
			{
				hiddenItemDefs.Add(allDef, value: true);
			}
		}
	}

	public void ClearHiddenDefs()
	{
		hiddenItemDefs.Clear();
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref hiddenItemDefs, "hiddenItemDefs", LookMode.Def, LookMode.Value);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		Dictionary<ThingDef, bool> dictionary = new Dictionary<ThingDef, bool>();
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef.hiddenWhileUndiscovered)
			{
				dictionary.Add(allDef, value: true);
			}
		}
		foreach (KeyValuePair<ThingDef, bool> hiddenItemDef in hiddenItemDefs)
		{
			if (dictionary.ContainsKey(hiddenItemDef.Key))
			{
				dictionary[hiddenItemDef.Key] = hiddenItemDef.Value;
			}
		}
		hiddenItemDefs = dictionary;
	}
}
