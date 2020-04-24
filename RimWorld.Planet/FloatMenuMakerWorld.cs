using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class FloatMenuMakerWorld
	{
		public static bool TryMakeFloatMenu(Caravan caravan)
		{
			if (!caravan.IsPlayerControlled)
			{
				return false;
			}
			Vector2 mousePositionOnUI = UI.MousePositionOnUI;
			List<FloatMenuOption> list = ChoicesAtFor(mousePositionOnUI, caravan);
			if (list.Count == 0)
			{
				return false;
			}
			FloatMenuWorld window = new FloatMenuWorld(list, caravan.LabelCap, mousePositionOnUI);
			Find.WindowStack.Add(window);
			return true;
		}

		public static List<FloatMenuOption> ChoicesAtFor(Vector2 mousePos, Caravan caravan)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<WorldObject> list2 = GenWorldUI.WorldObjectsUnderMouse(mousePos);
			for (int i = 0; i < list2.Count; i++)
			{
				list.AddRange(list2[i].GetFloatMenuOptions(caravan));
			}
			return list;
		}

		public static List<FloatMenuOption> ChoicesAtFor(int tile, Caravan caravan)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				if (allWorldObjects[i].Tile == tile)
				{
					list.AddRange(allWorldObjects[i].GetFloatMenuOptions(caravan));
				}
			}
			return list;
		}
	}
}
