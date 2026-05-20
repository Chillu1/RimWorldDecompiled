using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class WorldSelectionDrawer
{
	private static readonly Dictionary<WorldObject, float> selectTimes = new Dictionary<WorldObject, float>();

	private const float BaseSelectedTexJump = 25f;

	private const float BaseSelectedTexScale = 0.4f;

	private const float BaseSelectionRectSize = 35f;

	private static readonly Color HiddenSelectionBracketColor = new Color(1f, 1f, 1f, 0.35f);

	private static readonly Vector2[] bracketLocs = new Vector2[4];

	public static Dictionary<WorldObject, float> SelectTimes => selectTimes;

	public static void Notify_Selected(WorldObject t)
	{
		selectTimes[t] = Time.realtimeSinceStartup;
	}

	public static void Clear()
	{
		selectTimes.Clear();
	}

	public static void SelectionOverlaysOnGUI()
	{
		List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
		for (int i = 0; i < selectedObjects.Count; i++)
		{
			WorldObject worldObject = selectedObjects[i];
			DrawSelectionBracketOnGUIFor(worldObject);
			worldObject.ExtraSelectionOverlaysOnGUI();
		}
	}

	public static void DrawSelectionOverlays()
	{
		List<WorldObject> selectedObjects = Find.WorldSelector.SelectedObjects;
		for (int i = 0; i < selectedObjects.Count; i++)
		{
			selectedObjects[i].DrawExtraSelectionOverlays();
		}
	}

	private static void DrawSelectionBracketOnGUIFor(WorldObject obj)
	{
		Vector2 vector = obj.ScreenPos();
		SelectionDrawerUtility.CalculateSelectionBracketPositionsUI(rect: new Rect(vector.x - 17.5f, vector.y - 17.5f, 35f, 35f), textureSize: new Vector2((float)SelectionDrawerUtility.SelectedTexGUI.width * 0.4f, (float)SelectionDrawerUtility.SelectedTexGUI.height * 0.4f), bracketLocs: bracketLocs, obj: obj, selectTimes: selectTimes, jumpDistanceFactor: 25f);
		GUI.color = (obj.HiddenBehindTerrainNow() ? HiddenSelectionBracketColor : Color.white);
		int num = 90;
		for (int i = 0; i < 4; i++)
		{
			Widgets.DrawTextureRotated(bracketLocs[i], SelectionDrawerUtility.SelectedTexGUI, num, 0.4f);
			num += 90;
		}
		GUI.color = Color.white;
	}
}
