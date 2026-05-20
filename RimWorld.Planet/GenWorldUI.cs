using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class GenWorldUI
{
	private static List<Caravan> clickedCaravans = new List<Caravan>();

	private static List<WorldObject> clickedDynamicallyDrawnObjects = new List<WorldObject>();

	public static float CaravanDirectClickRadius => 0.35f * Find.WorldGrid.AverageTileSize;

	private static float CaravanWideClickRadius => 0.75f * Find.WorldGrid.AverageTileSize;

	private static float DynamicallyDrawnObjectDirectClickRadius => 0.35f * Find.WorldGrid.AverageTileSize;

	public static List<WorldObject> WorldObjectsUnderMouse(Vector2 mousePos)
	{
		List<WorldObject> list = new List<WorldObject>();
		ExpandableWorldObjectsUtility.GetExpandedWorldObjectUnderMouse(mousePos, list);
		float caravanDirectClickRadius = CaravanDirectClickRadius;
		clickedCaravans.Clear();
		List<Caravan> caravans = Find.WorldObjects.Caravans;
		for (int i = 0; i < caravans.Count; i++)
		{
			Caravan caravan = caravans[i];
			if (caravan.DistanceToMouse(mousePos) < caravanDirectClickRadius)
			{
				clickedCaravans.Add(caravan);
			}
		}
		clickedCaravans.SortBy((Caravan x) => x.DistanceToMouse(mousePos));
		for (int num = 0; num < clickedCaravans.Count; num++)
		{
			if (!list.Contains(clickedCaravans[num]))
			{
				list.Add(clickedCaravans[num]);
			}
		}
		float dynamicallyDrawnObjectDirectClickRadius = DynamicallyDrawnObjectDirectClickRadius;
		clickedDynamicallyDrawnObjects.Clear();
		List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
		for (int num2 = 0; num2 < allWorldObjects.Count; num2++)
		{
			WorldObject worldObject = allWorldObjects[num2];
			if (worldObject.def.useDynamicDrawer && worldObject.DistanceToMouse(mousePos) < dynamicallyDrawnObjectDirectClickRadius)
			{
				clickedDynamicallyDrawnObjects.Add(worldObject);
			}
		}
		clickedDynamicallyDrawnObjects.SortBy((WorldObject x) => x.DistanceToMouse(mousePos));
		for (int num3 = 0; num3 < clickedDynamicallyDrawnObjects.Count; num3++)
		{
			if (!list.Contains(clickedDynamicallyDrawnObjects[num3]))
			{
				list.Add(clickedDynamicallyDrawnObjects[num3]);
			}
		}
		PlanetTile planetTile = GenWorld.TileAt(mousePos);
		List<WorldObject> allWorldObjects2 = Find.WorldObjects.AllWorldObjects;
		for (int num4 = 0; num4 < allWorldObjects2.Count; num4++)
		{
			if (allWorldObjects2[num4].Tile == planetTile && !list.Contains(allWorldObjects2[num4]))
			{
				list.Add(allWorldObjects2[num4]);
			}
		}
		float caravanWideClickRadius = CaravanWideClickRadius;
		clickedCaravans.Clear();
		List<Caravan> caravans2 = Find.WorldObjects.Caravans;
		for (int num5 = 0; num5 < caravans2.Count; num5++)
		{
			Caravan caravan2 = caravans2[num5];
			if (caravan2.DistanceToMouse(mousePos) < caravanWideClickRadius)
			{
				clickedCaravans.Add(caravan2);
			}
		}
		clickedCaravans.SortBy((Caravan x) => x.DistanceToMouse(mousePos));
		for (int num6 = 0; num6 < clickedCaravans.Count; num6++)
		{
			if (!list.Contains(clickedCaravans[num6]))
			{
				list.Add(clickedCaravans[num6]);
			}
		}
		clickedCaravans.Clear();
		return list;
	}

	public static Vector2 WorldToUIPosition(Vector3 worldLoc)
	{
		Vector3 vector = Find.WorldCamera.WorldToScreenPoint(worldLoc) / Prefs.UIScale;
		return new Vector2(vector.x, (float)UI.screenHeight - vector.y);
	}

	public static float CurUITileSize()
	{
		Transform transform = Find.WorldCamera.transform;
		Vector3 localPosition = transform.localPosition;
		Quaternion rotation = transform.rotation;
		transform.localPosition = new Vector3(0f, 0f, localPosition.magnitude);
		transform.rotation = Quaternion.identity;
		float x = (WorldToUIPosition(new Vector3((0f - Find.WorldGrid.AverageTileSize) / 2f, 0f, 100f)) - WorldToUIPosition(new Vector3(Find.WorldGrid.AverageTileSize / 2f, 0f, 100f))).x;
		transform.localPosition = localPosition;
		transform.rotation = rotation;
		return x;
	}
}
