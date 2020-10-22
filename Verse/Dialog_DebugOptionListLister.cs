using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class Dialog_DebugOptionListLister : Dialog_DebugOptionLister
	{
		protected List<DebugMenuOption> options;

		protected override int HighlightedIndex
		{
			get
			{
				if (FilterAllows(options[prioritizedHighlightedIndex].label))
				{
					return prioritizedHighlightedIndex;
				}
				if (filter.NullOrEmpty())
				{
					return 0;
				}
				for (int i = 0; i < options.Count; i++)
				{
					if (FilterAllows(options[i].label))
					{
						currentHighlightIndex = i;
						break;
					}
				}
				return currentHighlightIndex;
			}
		}

		public Dialog_DebugOptionListLister(IEnumerable<DebugMenuOption> options)
		{
			this.options = options.ToList();
		}

		protected override void DoListingItems()
		{
			base.DoListingItems();
			int highlightedIndex = HighlightedIndex;
			for (int i = 0; i < options.Count; i++)
			{
				DebugMenuOption debugMenuOption = options[i];
				bool highlight = highlightedIndex == i;
				if (debugMenuOption.mode == DebugMenuOptionMode.Action)
				{
					DebugAction_NewTmp(debugMenuOption.label, debugMenuOption.method, highlight);
				}
				if (debugMenuOption.mode == DebugMenuOptionMode.Tool)
				{
					DebugToolMap_NewTmp(debugMenuOption.label, debugMenuOption.method, highlight);
				}
			}
		}

		protected override void ChangeHighlightedOption()
		{
			int highlightedIndex = HighlightedIndex;
			for (int i = 0; i < options.Count; i++)
			{
				int num = (highlightedIndex + i + 1) % options.Count;
				if (FilterAllows(options[num].label))
				{
					prioritizedHighlightedIndex = num;
					break;
				}
			}
		}

		public static void ShowSimpleDebugMenu<T>(IEnumerable<T> elements, Func<T, string> label, Action<T> chosen)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (T t in elements)
			{
				list.Add(new DebugMenuOption(label(t), DebugMenuOptionMode.Action, delegate
				{
					chosen(t);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		public override void OnAcceptKeyPressed()
		{
			if (!(GUI.GetNameOfFocusedControl() == "DebugFilter"))
			{
				return;
			}
			int highlightedIndex = HighlightedIndex;
			if (highlightedIndex >= 0)
			{
				Close();
				if (options[highlightedIndex].mode == DebugMenuOptionMode.Action)
				{
					options[highlightedIndex].method();
				}
				else
				{
					DebugTools.curTool = new DebugTool(options[highlightedIndex].label, options[highlightedIndex].method);
				}
			}
			Event.current.Use();
		}
	}
}
