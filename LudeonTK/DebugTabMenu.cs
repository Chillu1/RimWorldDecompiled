using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LudeonTK;

public abstract class DebugTabMenu
{
	protected DebugActionNode myRoot;

	public readonly DebugTabMenuDef def;

	protected readonly Dialog_Debug dialog;

	private List<DebugActionNode> cachedVisibleActions;

	public int Count => VisibleActions.Count;

	protected List<DebugActionNode> VisibleActions
	{
		get
		{
			if (cachedVisibleActions == null)
			{
				cachedVisibleActions = new List<DebugActionNode>();
				List<DebugActionNode> children = dialog.CurrentNode.children;
				for (int i = 0; i < children.Count; i++)
				{
					DebugActionNode debugActionNode = children[i];
					if (debugActionNode.VisibleNow)
					{
						cachedVisibleActions.Add(debugActionNode);
					}
				}
			}
			return cachedVisibleActions;
		}
	}

	public DebugTabMenu(DebugTabMenuDef def, Dialog_Debug dialog, DebugActionNode rootNode)
	{
		this.def = def;
		this.dialog = dialog;
	}

	public void Enter(DebugActionNode root)
	{
		myRoot = root;
		myRoot.Enter(dialog);
	}

	public abstract DebugActionNode InitActions(DebugActionNode root);

	public int HighlightedIndex(int currentHighlightIndex, int prioritizedHighlightedIndex)
	{
		List<DebugActionNode> visibleActions = VisibleActions;
		if (visibleActions.NullOrEmpty() || prioritizedHighlightedIndex >= visibleActions.Count)
		{
			return -1;
		}
		if (dialog.FilterAllows(visibleActions[prioritizedHighlightedIndex].LabelNow))
		{
			return prioritizedHighlightedIndex;
		}
		if (dialog.Filter.NullOrEmpty())
		{
			return 0;
		}
		for (int i = 0; i < visibleActions.Count; i++)
		{
			if (dialog.FilterAllows(visibleActions[i].LabelNow))
			{
				currentHighlightIndex = i;
				break;
			}
		}
		return currentHighlightIndex;
	}

	public string LabelAtIndex(int index)
	{
		return VisibleActions[index].LabelNow;
	}

	public void ListOptions(int highlightedIndex, float columnWidth)
	{
		string text = null;
		List<DebugActionNode> visibleActions = VisibleActions;
		for (int i = 0; i < visibleActions.Count; i++)
		{
			DebugActionNode debugActionNode = visibleActions[i];
			if (dialog.FilterAllows(debugActionNode.LabelNow))
			{
				if (debugActionNode.category != text)
				{
					dialog.DoGap();
					dialog.DoLabel(debugActionNode.category, columnWidth);
					text = debugActionNode.category;
				}
				Log.openOnMessage = true;
				try
				{
					dialog.DrawNode(debugActionNode, columnWidth, highlightedIndex == i);
				}
				finally
				{
					Log.openOnMessage = false;
				}
			}
		}
	}

	public void Recache()
	{
		cachedVisibleActions = null;
	}

	public void OnAcceptKeyPressed(int index)
	{
		if (index >= 0)
		{
			VisibleActions[index].Enter(dialog);
		}
	}

	public static DebugTabMenu CreateMenu(DebugTabMenuDef def, Dialog_Debug dialog, DebugActionNode root)
	{
		return (DebugTabMenu)Activator.CreateInstance(def.menuClass, def, dialog, root);
	}
}
