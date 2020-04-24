using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class GenWorldUI
	{
		private static List<Caravan> clickedCaravans = new List<Caravan>();

		private static List<WorldObject> clickedDynamicallyDrawnObjects = new List<WorldObject>();

		public static float CaravanDirectClickRadius => 0.35f * Find.WorldGrid.averageTileSize;

		private static float CaravanWideClickRadius => 0.75f * Find.WorldGrid.averageTileSize;

		private static float DynamicallyDrawnObjectDirectClickRadius => 0.35f * Find.WorldGrid.averageTileSize;

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
			for (int j = 0; j < clickedCaravans.Count; j++)
			{
				if (!list.Contains(clickedCaravans[j]))
				{
					list.Add(clickedCaravans[j]);
				}
			}
			float dynamicallyDrawnObjectDirectClickRadius = DynamicallyDrawnObjectDirectClickRadius;
			clickedDynamicallyDrawnObjects.Clear();
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int k = 0; k < allWorldObjects.Count; k++)
			{
				WorldObject worldObject = allWorldObjects[k];
				if (worldObject.def.useDynamicDrawer && worldObject.DistanceToMouse(mousePos) < dynamicallyDrawnObjectDirectClickRadius)
				{
					clickedDynamicallyDrawnObjects.Add(worldObject);
				}
			}
			clickedDynamicallyDrawnObjects.SortBy((WorldObject x) => x.DistanceToMouse(mousePos));
			for (int l = 0; l < clickedDynamicallyDrawnObjects.Count; l++)
			{
				if (!list.Contains(clickedDynamicallyDrawnObjects[l]))
				{
					list.Add(clickedDynamicallyDrawnObjects[l]);
				}
			}
			int num = GenWorld.TileAt(mousePos);
			List<WorldObject> allWorldObjects2 = Find.WorldObjects.AllWorldObjects;
			for (int m = 0; m < allWorldObjects2.Count; m++)
			{
				if (allWorldObjects2[m].Tile == num && !list.Contains(allWorldObjects2[m]))
				{
					list.Add(allWorldObjects2[m]);
				}
			}
			float caravanWideClickRadius = CaravanWideClickRadius;
			clickedCaravans.Clear();
			List<Caravan> caravans2 = Find.WorldObjects.Caravans;
			for (int n = 0; n < caravans2.Count; n++)
			{
				Caravan caravan2 = caravans2[n];
				if (caravan2.DistanceToMouse(mousePos) < caravanWideClickRadius)
				{
					clickedCaravans.Add(caravan2);
				}
			}
			clickedCaravans.SortBy((Caravan x) => x.DistanceToMouse(mousePos));
			for (int num2 = 0; num2 < clickedCaravans.Count; num2++)
			{
				if (!list.Contains(clickedCaravans[num2]))
				{
					list.Add(clickedCaravans[num2]);
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
			Vector3 localPosition = Find.WorldCamera.transform.localPosition;
			Quaternion rotation = Find.WorldCamera.transform.rotation;
			Find.WorldCamera.transform.localPosition = new Vector3(0f, 0f, localPosition.magnitude);
			Find.WorldCamera.transform.rotation = Quaternion.identity;
			float x = (WorldToUIPosition(new Vector3((0f - Find.WorldGrid.averageTileSize) / 2f, 0f, 100f)) - WorldToUIPosition(new Vector3(Find.WorldGrid.averageTileSize / 2f, 0f, 100f))).x;
			Find.WorldCamera.transform.localPosition = localPosition;
			Find.WorldCamera.transform.rotation = rotation;
			return x;
		}
	}
}
