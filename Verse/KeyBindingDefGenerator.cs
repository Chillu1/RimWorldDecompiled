using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse
{
	public static class KeyBindingDefGenerator
	{
		public static IEnumerable<KeyBindingCategoryDef> ImpliedKeyBindingCategoryDefs()
		{
			List<KeyBindingCategoryDef> gameUniversalCats = DefDatabase<KeyBindingCategoryDef>.AllDefs.Where((KeyBindingCategoryDef d) => d.isGameUniversal).ToList();
			foreach (DesignationCategoryDef allDef in DefDatabase<DesignationCategoryDef>.AllDefs)
			{
				KeyBindingCategoryDef keyBindingCategoryDef = new KeyBindingCategoryDef();
				keyBindingCategoryDef.defName = "Architect_" + allDef.defName;
				keyBindingCategoryDef.label = allDef.label + " tab";
				keyBindingCategoryDef.description = "Key bindings for the \"" + allDef.LabelCap + "\" section of the Architect menu";
				keyBindingCategoryDef.modContentPack = allDef.modContentPack;
				keyBindingCategoryDef.checkForConflicts.AddRange(gameUniversalCats);
				for (int i = 0; i < gameUniversalCats.Count; i++)
				{
					gameUniversalCats[i].checkForConflicts.Add(keyBindingCategoryDef);
				}
				allDef.bindingCatDef = keyBindingCategoryDef;
				yield return keyBindingCategoryDef;
			}
		}

		public static IEnumerable<KeyBindingDef> ImpliedKeyBindingDefs()
		{
			foreach (MainButtonDef item in DefDatabase<MainButtonDef>.AllDefs.OrderBy((MainButtonDef td) => td.order))
			{
				if (item.defaultHotKey != 0)
				{
					KeyBindingDef keyBindingDef = new KeyBindingDef();
					keyBindingDef.label = "Toggle " + item.label + " tab";
					keyBindingDef.defName = "MainTab_" + item.defName;
					keyBindingDef.category = KeyBindingCategoryDefOf.MainTabs;
					keyBindingDef.defaultKeyCodeA = item.defaultHotKey;
					keyBindingDef.modContentPack = item.modContentPack;
					item.hotKey = keyBindingDef;
					yield return keyBindingDef;
				}
			}
		}
	}
}
