using UnityEngine;

namespace Verse
{
	public static class GenView
	{
		private static CellRect viewRect;

		private const int ViewRectMargin = 5;

		public static bool ShouldSpawnMotesAt(this Vector3 loc, Map map)
		{
			return loc.ToIntVec3().ShouldSpawnMotesAt(map);
		}

		public static bool ShouldSpawnMotesAt(this IntVec3 loc, Map map)
		{
			if (map != Find.CurrentMap)
			{
				return false;
			}
			if (!loc.InBounds(map))
			{
				return false;
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
