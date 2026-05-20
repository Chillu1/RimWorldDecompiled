using System;
using Verse;

namespace RimWorld;

public class QuickSearchFilter
{
	private string inputText = "";

	private string searchText = "";

	private readonly LRUCache<string, bool> cachedMatches = new LRUCache<string, bool>(5000);

	public string Text
	{
		get
		{
			return inputText;
		}
		set
		{
			inputText = value;
			searchText = value.Trim();
			cachedMatches.Clear();
		}
	}

	public bool Active => !inputText.NullOrEmpty();

	public bool Matches(string value)
	{
		if (!Active)
		{
			return true;
		}
		if (value.NullOrEmpty())
		{
			return false;
		}
		if (!cachedMatches.TryGetValue(value, out var result))
		{
			result = MatchImpl(value);
			cachedMatches.Add(value, result);
		}
		return result;
	}

	private bool MatchImpl(string value)
	{
		return value.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) != -1;
	}

	public bool Matches(ThingDef td)
	{
		if (!Find.HiddenItemsManager.Hidden(td))
		{
			return Matches(td.label);
		}
		return false;
	}

	public bool Matches(SpecialThingFilterDef sfDef)
	{
		return Matches(sfDef.label);
	}
}
