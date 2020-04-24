using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class SelectionDrawer
	{
		private static Dictionary<object, float> selectTimes = new Dictionary<object, float>();

		private static readonly Material SelectionBracketMat = MaterialPool.MatFrom("UI/Overlays/SelectionBracket", ShaderDatabase.MetaOverlay);

		private static Vector3[] bracketLocs = new Vector3[4];

		public static Dictionary<object, float> SelectTimes => selectTimes;

		public static void Notify_Selected(object t)
		{
			selectTimes[t] = Time.realtimeSinceStartup;
		}

		public static void Clear()
		{
			selectTimes.Clear();
		}

		public static void DrawSelectionOverlays()
		{
			foreach (object selectedObject in Find.Selector.SelectedObjects)
			{
				DrawSelectionBracketFor(selectedObject);
				(selectedObject as Thing)?.DrawExtraSelectionOverlays();
			}
		}

		private static void DrawSelectionBracketFor(object obj)
		{
			Zone zone = obj as Zone;
			if (zone != null)
			{
				GenDraw.DrawFieldEdges(zone.Cells);
			}
			Thing thing = obj as Thing;
			if (thing != null)
			{
				CellRect? customRectForSelector = thing.CustomRectForSelector;
				if (customRectForSelector.HasValue)
				{
					SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld(bracketLocs, thing, customRectForSelector.Value.CenterVector3, new Vector2(customRectForSelector.Value.Width, customRectForSelector.Value.Height), selectTimes, Vector2.one);
				}
				else
				{
					SelectionDrawerUtility.CalculateSelectionBracketPositionsWorld(bracketLocs, thing, thing.DrawPos, thing.RotatedSize.ToVector2(), selectTimes, Vector2.one);
				}
				int num = 0;
				for (int i = 0; i < 4; i++)
				{
					Quaternion rotation = Quaternion.AngleAxis(num, Vector3.up);
					Graphics.DrawMesh(MeshPool.plane10, bracketLocs[i], rotation, SelectionBracketMat, 0);
					num -= 90;
				}
			}
		}
	}
}
