using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace LudeonTK;

public class Dialog_DebugOptionListLister : Dialog_DebugOptionLister
{
	protected List<DebugMenuOption> options;

	protected string header;

	protected override int HighlightedIndex
	{
		get
		{
			if (options.NullOrEmpty())
			{
				return base.HighlightedIndex;
			}
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

	public Dialog_DebugOptionListLister(IEnumerable<DebugMenuOption> options, string header = null)
	{
		this.options = options.ToList();
		this.header = header;
	}

	protected override void DoListingItems(Rect inRect, float columnWidth)
	{
		if (KeyBindingDefOf.Dev_ChangeSelectedDebugAction.KeyDownEvent)
		{
			ChangeHighlightedOption();
		}
		if (!string.IsNullOrEmpty(header))
		{
			DebugLabel(header, columnWidth);
		}
		int highlightedIndex = HighlightedIndex;
		for (int i = 0; i < options.Count; i++)
		{
			DebugMenuOption debugMenuOption = options[i];
			bool highlight = highlightedIndex == i;
			switch (debugMenuOption.mode)
			{
			case DebugMenuOptionMode.Action:
				DebugAction(debugMenuOption.label, columnWidth, debugMenuOption.method, highlight);
				break;
			case DebugMenuOptionMode.Tool:
				DebugToolMap(debugMenuOption.label, columnWidth, debugMenuOption.method, highlight);
				break;
			}
		}
	}

	protected override void ChangeHighlightedOption()
	{
		int highlightedIndex = HighlightedIndex;
		for (int i = 0; i < options.Count; i++)
		{
			int index = (highlightedIndex + i + 1) % options.Count;
			if (FilterAllows(options[index].label))
			{
				prioritizedHighlightedIndex = index;
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
