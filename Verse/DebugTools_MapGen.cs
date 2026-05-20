using System;
using System.Collections.Generic;
using LudeonTK;

namespace Verse;

public static class DebugTools_MapGen
{
	public static List<DebugActionNode> Options_Scatterers()
	{
		List<DebugActionNode> list = new List<DebugActionNode>();
		foreach (Type item in typeof(GenStep_Scatterer).AllLeafSubclasses())
		{
			Type localSt = item;
			list.Add(new DebugActionNode(localSt.ToString(), DebugActionType.ToolMap)
			{
				action = delegate
				{
					((GenStep_Scatterer)Activator.CreateInstance(localSt)).ForceScatterAt(UI.MouseCell(), Find.CurrentMap);
				}
			});
		}
		return list;
	}
}
