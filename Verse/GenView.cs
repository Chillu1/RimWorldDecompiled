using UnityEngine;

namespace Verse
{
	public static class GenView
	{
		private static CellRect viewRect;

		private const int ViewRectMargin = 5;

		public static bool ShouldSpawnMotesAt(this Vector3 loc, Map map, bool drawOffscreen = true)
		{
			return loc.ToIntVec3().ShouldSpawnMotesAt(map, drawOffscreen);
		}

		public static bool ShouldSpawnMotesAt(this IntVec3 loc, Map map, bool drawOffscreen = true)
		{
			if (map != Find.CurrentMap)
			{
				return false;
			}
			if (!loc.InBounds(map))
			{
				return false;
			}
			if (drawOffscreen)
			{
				return true;
			}
			viewRect = Find.CameraDriver.CurrentViewRect;
			viewRect = viewRect.ExpandedBy(5);
			return viewRect.Contains(loc);
		}

		public static Vector3 RandomPositionOnOrNearScreen()
		{
			viewRect = Find.CameraDriver.CurrentViewRect;
			viewRect = viewRect.ExpandedBy(5);
			viewRect.ClipInsideMap(Find.CurrentMap);
			return viewRect.RandomVector3;
		}
	}
}
