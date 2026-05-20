using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class WorldObjectSelectionUtility
{
	public static IEnumerable<WorldObject> MultiSelectableWorldObjectsInScreenRectDistinct(Rect rect)
	{
		List<WorldObject> allObjects = Find.WorldObjects.AllWorldObjects;
		for (int i = 0; i < allObjects.Count; i++)
		{
			if (allObjects[i].NeverMultiSelect || allObjects[i].HiddenBehindTerrainNow())
			{
				continue;
			}
			if (ExpandableWorldObjectsUtility.IsExpanded(allObjects[i]))
			{
				if (rect.Overlaps(ExpandableWorldObjectsUtility.ExpandedIconScreenRect(allObjects[i])))
				{
					yield return allObjects[i];
				}
			}
			else if (rect.Contains(allObjects[i].ScreenPos()))
			{
				yield return allObjects[i];
			}
		}
	}

	public static bool HiddenBehindTerrainNow(this WorldObject o)
	{
		if (!WorldRendererUtility.HiddenBehindTerrainNow(o.DrawPos))
		{
			if (!o.VisibleInBackground)
			{
				return o.Tile.Layer != PlanetLayer.Selected;
			}
			return false;
		}
		return true;
	}

	public static Vector2 ScreenPos(this WorldObject o)
	{
		return GenWorldUI.WorldToUIPosition(o.DrawPos);
	}

	public static bool VisibleToCameraNow(this WorldObject o)
	{
		if (!WorldRendererUtility.WorldSelected)
		{
			return false;
		}
		if (o.HiddenBehindTerrainNow())
		{
			return false;
		}
		Vector2 point = o.ScreenPos();
		return new Rect(0f, 0f, UI.screenWidth, UI.screenHeight).Contains(point);
	}

	public static float DistanceToMouse(this WorldObject o, Vector2 mousePos)
	{
		Ray ray = Find.WorldCamera.ScreenPointToRay(mousePos * Prefs.UIScale);
		int worldLayerMask = WorldCameraManager.WorldLayerMask;
		if (Physics.Raycast(ray, out var hitInfo, 1500f, worldLayerMask))
		{
			return Vector3.Distance(hitInfo.point, o.DrawPos);
		}
		return Vector3.Cross(ray.direction, o.DrawPos - ray.origin).magnitude;
	}
}
