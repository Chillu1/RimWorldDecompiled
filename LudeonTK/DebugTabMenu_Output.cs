using System;
using System.Reflection;
using RimWorld;
using Verse;

namespace LudeonTK;

public class DebugTabMenu_Output : DebugTabMenu
{
	public const string DefaultCategory = "General";

	public DebugTabMenu_Output(DebugTabMenuDef def, Dialog_Debug dialog, DebugActionNode root)
		: base(def, dialog, root)
	{
	}

	public override DebugActionNode InitActions(DebugActionNode absRoot)
	{
		myRoot = new DebugActionNode("Outputs");
		absRoot.AddChild(myRoot);
		foreach (Type allType in GenTypes.AllTypes)
		{
			MethodInfo[] methods = allType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.TryGetAttribute<DebugOutputAttribute>(out var customAttribute))
				{
					GenerateCacheForMethod(methodInfo, customAttribute);
				}
			}
		}
		myRoot.TrySort();
		return myRoot;
	}

	private void GenerateCacheForMethod(MethodInfo method, DebugOutputAttribute attribute)
	{
		string label = attribute.name ?? GenText.SplitCamelCase(method.Name);
		Action action = Delegate.CreateDelegate(typeof(Action), method) as Action;
		DebugActionNode debugActionNode = new DebugActionNode(label, DebugActionType.Action, action);
		debugActionNode.category = attribute.category ?? "General";
		debugActionNode.visibilityGetter = () => !attribute.onlyWhenPlaying || Current.ProgramState == ProgramState.Playing;
		myRoot.AddChild(debugActionNode);
	}
}
