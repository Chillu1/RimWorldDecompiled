using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace LudeonTK;

public class DebugActionNode
{
	public string label;

	public DebugActionType actionType;

	public Action action;

	public Action<Pawn> pawnAction;

	public Func<List<DebugActionNode>> childGetter;

	public DebugActionAttribute sourceAttribute;

	public Func<bool> visibilityGetter;

	public Func<string> labelGetter;

	public string category;

	public int displayPriority;

	public FieldInfo settingsField;

	private bool childrenSetup;

	public DebugActionNode parent;

	public List<DebugActionNode> children = new List<DebugActionNode>();

	private string cachedPath;

	private string cachedLabelNow;

	private bool sorted;

	private bool? cachedCheckOn;

	private int lastLabelCacheFrame = -1;

	private const int LabelRecacheFrameCount = 30;

	public bool IsRoot => parent == null;

	public bool On
	{
		get
		{
			if (!cachedCheckOn.HasValue)
			{
				if (settingsField == null)
				{
					cachedCheckOn = false;
				}
				else
				{
					cachedCheckOn = (bool)settingsField.GetValue(null);
				}
			}
			return cachedCheckOn.Value;
		}
	}

	public string LabelNow
	{
		get
		{
			if (Time.frameCount >= lastLabelCacheFrame + 30)
			{
				DirtyLabelCache();
			}
			if (cachedLabelNow == null)
			{
				if (labelGetter != null)
				{
					try
					{
						cachedLabelNow = labelGetter();
					}
					catch (Exception ex)
					{
						Log.Error("Exception getting label for DebugActionNode: " + ex);
						cachedLabelNow = null;
						return label;
					}
				}
				else
				{
					cachedLabelNow = label;
				}
			}
			return cachedLabelNow;
		}
	}

	public bool ActiveNow
	{
		get
		{
			switch (actionType)
			{
			case DebugActionType.ToolWorld:
				return WorldRendererUtility.WorldSelected;
			case DebugActionType.ToolMap:
			case DebugActionType.ToolMapForPawns:
				return WorldRendererUtility.DrawingMap;
			default:
				return true;
			}
		}
	}

	public bool VisibleNow
	{
		get
		{
			if (sourceAttribute != null && !sourceAttribute.IsAllowedInCurrentGameState)
			{
				return false;
			}
			if (visibilityGetter != null && !visibilityGetter())
			{
				return false;
			}
			return true;
		}
	}

	public string Path
	{
		get
		{
			if (cachedPath == null)
			{
				if (parent != null && !parent.IsRoot)
				{
					cachedPath = parent.Path + "\\" + label;
				}
				else
				{
					cachedPath = label;
				}
			}
			return cachedPath;
		}
	}

	public DebugActionNode()
	{
	}

	public DebugActionNode(string label, DebugActionType actionType = DebugActionType.Action, Action action = null, Action<Pawn> pawnAction = null)
	{
		this.label = label;
		this.actionType = actionType;
		this.action = action;
		this.pawnAction = pawnAction;
	}

	public void AddChild(DebugActionNode child)
	{
		child.SetParent(this);
		children.Add(child);
		sorted = false;
	}

	public void SetParent(DebugActionNode newParent)
	{
		parent = newParent;
		DirtyPath();
	}

	private void DirtyPath()
	{
		cachedPath = null;
		foreach (DebugActionNode child in children)
		{
			child.DirtyPath();
		}
	}

	public void DirtyLabelCache()
	{
		cachedLabelNow = null;
		cachedCheckOn = null;
		lastLabelCacheFrame = Time.frameCount;
	}

	public void TrySort()
	{
		if (sorted)
		{
			return;
		}
		if (!children.NullOrEmpty())
		{
			children = (from r in children
				orderby DebugActionCategories.GetOrderFor(r.category), r.category, -r.displayPriority
				select r).ToList();
		}
		sorted = true;
	}

	public void TrySetupChildren()
	{
		if (!childrenSetup && childGetter != null)
		{
			foreach (DebugActionNode item in childGetter())
			{
				AddChild(item);
			}
		}
		childrenSetup = true;
	}

	private static IEnumerable<Pawn> PawnsInside(IThingHolder holder)
	{
		if (holder is Pawn pawn)
		{
			yield return pawn;
		}
		ThingOwner directlyHeldThings = holder.GetDirectlyHeldThings();
		if (directlyHeldThings == null)
		{
			yield break;
		}
		foreach (Thing item in (IEnumerable<Thing>)directlyHeldThings)
		{
			if (!(item is IThingHolder holder2))
			{
				continue;
			}
			foreach (Pawn item2 in PawnsInside(holder2))
			{
				yield return item2;
			}
		}
	}

	private static IEnumerable<Pawn> PawnsAt(IntVec3 cell, Map map)
	{
		foreach (Thing item in map.thingGrid.ThingsAt(cell))
		{
			if (!(item is IThingHolder holder))
			{
				continue;
			}
			foreach (Pawn item2 in PawnsInside(holder))
			{
				yield return item2;
			}
		}
	}

	public void Enter(Dialog_Debug dialog)
	{
		TrySetupChildren();
		if (children.Any())
		{
			TrySort();
			if (dialog == null)
			{
				dialog = new Dialog_Debug();
				Find.WindowStack.Add(dialog);
			}
			dialog.SetCurrentNode(this);
			return;
		}
		dialog?.Close();
		switch (actionType)
		{
		case DebugActionType.Action:
			action();
			break;
		case DebugActionType.ToolMap:
		case DebugActionType.ToolWorld:
			DebugTools.curTool = new DebugTool(LabelNow, action);
			break;
		case DebugActionType.ToolMapForPawns:
		{
			DebugActionNode instance = this;
			DebugTools.curTool = new DebugTool(LabelNow, delegate
			{
				if (UI.MouseCell().InBounds(Find.CurrentMap))
				{
					foreach (Pawn item in PawnsAt(UI.MouseCell(), Find.CurrentMap).ToList())
					{
						instance.pawnAction(item);
					}
				}
			});
			break;
		}
		}
		DirtyLabelCache();
	}
}
