using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace LudeonTK;

public class DebugTabMenu_Actions : DebugTabMenu
{
	private DebugActionNode moreActionsNode;

	public DebugTabMenu_Actions(DebugTabMenuDef def, Dialog_Debug dialog, DebugActionNode root)
		: base(def, dialog, root)
	{
	}

	public override DebugActionNode InitActions(DebugActionNode absRoot)
	{
		myRoot = new DebugActionNode("Actions");
		absRoot.AddChild(myRoot);
		moreActionsNode = new DebugActionNode("Show more actions")
		{
			category = "More debug actions",
			displayPriority = -999999
		};
		myRoot.AddChild(moreActionsNode);
		foreach (Type allType in GenTypes.AllTypes)
		{
			MethodInfo[] methods = allType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.TryGetAttribute<DebugActionAttribute>(out var customAttribute))
				{
					GenerateCacheForMethod(methodInfo, customAttribute);
				}
				if (!methodInfo.TryGetAttribute<DebugActionYielderAttribute>(out var _))
				{
					continue;
				}
				foreach (DebugActionNode item in (IEnumerable<DebugActionNode>)methodInfo.Invoke(null, null))
				{
					myRoot.AddChild(item);
				}
			}
		}
		myRoot.TrySort();
		return myRoot;
	}

	private void GenerateCacheForMethod(MethodInfo method, DebugActionAttribute attribute)
	{
		string text = (string.IsNullOrEmpty(attribute.name) ? GenText.SplitCamelCase(method.Name) : attribute.name);
		if (attribute.actionType == DebugActionType.ToolMap || attribute.actionType == DebugActionType.ToolMapForPawns || attribute.actionType == DebugActionType.ToolWorld)
		{
			text = "T: " + text;
		}
		Type returnType = method.ReturnType;
		DebugActionNode debugActionNode;
		if (returnType == typeof(List<DebugActionNode>))
		{
			debugActionNode = new DebugActionNode();
			debugActionNode.childGetter = Delegate.CreateDelegate(typeof(Func<List<DebugActionNode>>), method) as Func<List<DebugActionNode>>;
			if (!text.EndsWith("..."))
			{
				text += "...";
			}
		}
		else if (returnType == typeof(DebugActionNode))
		{
			debugActionNode = (DebugActionNode)method.Invoke(null, null);
			if (debugActionNode.children.Any() && !text.EndsWith("..."))
			{
				text += "...";
			}
		}
		else
		{
			debugActionNode = new DebugActionNode();
			if (attribute.actionType == DebugActionType.ToolMapForPawns)
			{
				debugActionNode.pawnAction = Delegate.CreateDelegate(typeof(Action<Pawn>), method) as Action<Pawn>;
			}
			else
			{
				debugActionNode.action = Delegate.CreateDelegate(typeof(Action), method) as Action;
			}
		}
		if (debugActionNode.label.NullOrEmpty())
		{
			debugActionNode.label = text;
		}
		debugActionNode.label = debugActionNode.label.Replace("\\", "/");
		debugActionNode.category = attribute.category ?? "General";
		debugActionNode.actionType = attribute.actionType;
		debugActionNode.displayPriority = attribute.displayPriority;
		debugActionNode.sourceAttribute = attribute;
		if (attribute.hideInSubMenu)
		{
			moreActionsNode.AddChild(debugActionNode);
		}
		else
		{
			myRoot.AddChild(debugActionNode);
		}
	}
}
