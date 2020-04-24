using System;
using System.Collections.Generic;

namespace Verse
{
	public static class DebugTools_MapGen
	{
		public static List<DebugMenuOption> Options_Scatterers()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type item in typeof(GenStep_Scatterer).AllLeafSubclasses())
			{
				Type localSt = item;
				list.Add(new DebugMenuOption(localSt.ToString(), DebugMenuOptionMode.Tool, delegate
				{
					((GenStep_Scatterer)Activator.CreateInstance(localSt)).ForceScatterAt(UI.MouseCell(), Find.CurrentMap);
				}));
			}
			return list;
		}
	}
}
