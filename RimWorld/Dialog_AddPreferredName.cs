using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_AddPreferredName : Window
{
	private string searchName = "";

	private string[] searchWords;

	private List<NameTriple> cachedNames;

	private static readonly List<NameTriple> Blacklist = new List<NameTriple>();

	private static readonly List<NameTriple> cachedSearch = new List<NameTriple>();

	private string lastSearch;

	public override Vector2 InitialSize => new Vector2(400f, 650f);

	public Dialog_AddPreferredName()
	{
		doCloseButton = true;
		absorbInputAroundWindow = true;
		lastSearch = "";
		cachedNames = (from n in SolidBioDatabase.allBios.Select((PawnBio b) => b.name).Concat(PawnNameDatabaseSolid.AllNames())
			orderby n.Last descending
			select n).ToList();
		CacheBlacklist();
	}

	public override void DoWindowContents(Rect inRect)
	{
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.Begin(inRect);
		listing_Standard.Label("TypeFirstNickOrLastName".Translate());
		string text = listing_Standard.TextEntry(searchName);
		if (text.Length < 20)
		{
			searchName = text;
			searchWords = searchName.Replace("'", "").Split(' ');
		}
		listing_Standard.Gap(4f);
		int num = Mathf.FloorToInt((inRect.height - (Window.CloseButSize.y + 8f) - listing_Standard.CurHeight) / (30f + listing_Standard.verticalSpacing));
		if (searchName.Length > 1)
		{
			if (searchName != lastSearch)
			{
				lastSearch = searchName;
				cachedSearch.Clear();
				foreach (NameTriple item in cachedNames.Where(FilterMatch))
				{
					cachedSearch.Add(item);
					if (listing_Standard.ButtonText(item.ToString()))
					{
						TryChooseName(item);
					}
					if (cachedSearch.Count >= num)
					{
						break;
					}
				}
			}
			foreach (NameTriple item2 in cachedSearch)
			{
				if (listing_Standard.ButtonText(item2.ToString()))
				{
					TryChooseName(item2);
				}
			}
		}
		listing_Standard.End();
	}

	private bool FilterMatch(NameTriple n)
	{
		if (IsBlacklisted(n))
		{
			return false;
		}
		if (searchWords.Length == 0)
		{
			return false;
		}
		if (searchWords.Length == 1)
		{
			if (!n.Last.StartsWith(searchName, StringComparison.OrdinalIgnoreCase) && !n.First.StartsWith(searchName, StringComparison.OrdinalIgnoreCase))
			{
				return n.Nick.StartsWith(searchName, StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		if (searchWords.Length > 1)
		{
			string value = searchName.Replace("'", "");
			if (n.First.IndexOf(' ') >= 0)
			{
				return n.First.StartsWith(value, StringComparison.OrdinalIgnoreCase);
			}
			if (n.Last.IndexOf(' ') >= 0)
			{
				return n.Last.StartsWith(value, StringComparison.OrdinalIgnoreCase);
			}
			if (n.Nick.IndexOf(' ') >= 0)
			{
				return n.Nick.StartsWith(value, StringComparison.OrdinalIgnoreCase);
			}
		}
		if (searchWords.Length == 2)
		{
			if (n.First.EqualsIgnoreCase(searchWords[0]))
			{
				if (!n.Last.StartsWith(searchWords[1], StringComparison.OrdinalIgnoreCase))
				{
					return n.Nick.StartsWith(searchWords[1], StringComparison.OrdinalIgnoreCase);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private void TryChooseName(NameTriple name)
	{
		if (AlreadyPreferred(name))
		{
			Messages.Message("MessageAlreadyPreferredName".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		Prefs.PreferredNames.Add(name.ToString());
		Close();
	}

	private bool AlreadyPreferred(NameTriple name)
	{
		return Prefs.PreferredNames.Contains(name.ToString());
	}

	private void CacheBlacklist()
	{
		if (!Blacklist.Any())
		{
			Blacklist.AddRange((from b in SolidBioDatabase.allBios
				where b.rare
				select b.name).ToList());
		}
	}

	private static bool IsBlacklisted(NameTriple triple)
	{
		foreach (NameTriple item in Blacklist)
		{
			if (item.First.EqualsIgnoreCase(triple.First) && item.Last.EqualsIgnoreCase(triple.Last))
			{
				return true;
			}
		}
		return false;
	}
}
