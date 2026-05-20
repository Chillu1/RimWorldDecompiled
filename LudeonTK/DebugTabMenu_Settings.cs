using System.Reflection;
using RimWorld;
using Verse;

namespace LudeonTK;

public class DebugTabMenu_Settings : DebugTabMenu
{
	public DebugTabMenu_Settings(DebugTabMenuDef def, Dialog_Debug dialog, DebugActionNode root)
		: base(def, dialog, root)
	{
	}

	public override DebugActionNode InitActions(DebugActionNode absRoot)
	{
		myRoot = new DebugActionNode("Settings");
		absRoot.AddChild(myRoot);
		FieldInfo[] fields = typeof(DebugSettings).GetFields();
		foreach (FieldInfo fi in fields)
		{
			AddNode(fi, "General");
		}
		fields = typeof(DebugViewSettings).GetFields();
		foreach (FieldInfo fi2 in fields)
		{
			AddNode(fi2, "View");
		}
		fields = typeof(DebugGenerationSettings).GetFields();
		foreach (FieldInfo fi3 in fields)
		{
			AddNode(fi3, "Generation");
		}
		return myRoot;
	}

	private void AddNode(FieldInfo fi, string categoryLabel)
	{
		if (fi.IsLiteral)
		{
			return;
		}
		DebugActionNode debugActionNode = new DebugActionNode(LegibleFieldName(fi), DebugActionType.Action, delegate
		{
			bool flag = (bool)fi.GetValue(null);
			fi.SetValue(null, !flag);
			MethodInfo method = fi.DeclaringType.GetMethod(fi.Name + "Toggled", BindingFlags.Static | BindingFlags.Public);
			if (method != null)
			{
				method.Invoke(null, null);
			}
		});
		debugActionNode.category = categoryLabel;
		debugActionNode.settingsField = fi;
		myRoot.AddChild(debugActionNode);
	}

	private string LegibleFieldName(FieldInfo fi)
	{
		return GenText.SplitCamelCase(fi.Name).CapitalizeFirst();
	}
}
