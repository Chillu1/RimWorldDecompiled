using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class BundleLetter : Letter
	{
		private List<Letter> bundledLetters = new List<Letter>();

		private List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();

		public override bool CanDismissWithRightClick => false;

		public void SetLetters(List<Letter> letters)
		{
			if (GenCollection.ListsEqual(letters, bundledLetters))
			{
				return;
			}
			bundledLetters.Clear();
			bundledLetters.AddRange(letters);
			floatMenuOptions.Clear();
			foreach (Letter letter in letters)
			{
				FloatMenuOption item = new FloatMenuOption(letter.Label, delegate
				{
					letter.OpenLetter();
				});
				floatMenuOptions.Add(item);
			}
			base.Label = bundledLetters.Count + " " + "MoreLower".Translate() + "...";
		}

		public override void OpenLetter()
		{
			if (Event.current.button == 0)
			{
				Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
			}
		}

		protected override string GetMouseoverText()
		{
			return "MoreLetters".Translate(bundledLetters.Count) + ":\n\n" + bundledLetters.Select((Letter l) => l.Label.ToString()).ToLineList(" - ");
		}
	}
}
