using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_AddPreferredName : Window
	{
		private string searchName = "";

		private string[] searchWords;

		private List<NameTriple> cachedNames;

		public override Vector2 InitialSize => new Vector2(400f, 650f);

		public Dialog_AddPreferredName()
		{
			doCloseButton = true;
			absorbInputAroundWindow = true;
			cachedNames = (from n in SolidBioDatabase.allBios.Select((PawnBio b) => b.name).Concat(PawnNameDatabaseSolid.AllNames())
				orderby n.Last descending
				select n).ToList();
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
			if (searchName.Length > 1)
			{
				foreach (NameTriple item in cachedNames.Where(FilterMatch))
				{
					if (listing_Standard.ButtonText(item.ToString()))
					{
						TryChooseName(item);
					}
					if (listing_Standard.CurHeight + 30f > inRect.height - (CloseButSize.y + 8f))
					{
						break;
					}
				}
			}
			listing_Standard.End();
		}

		private bool FilterMatch(NameTriple n)
		{
			if (n.First == "Tynan" && n.Last == "Sylvester")
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
	}
}
