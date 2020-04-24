using Verse.AI;

namespace Verse
{
	public static class ReachabilityWithinRegion
	{
		public static bool ThingFromRegionListerReachable(Thing thing, Region region, PathEndMode peMode, Pawn traveler)
		{
			Map map = region.Map;
			if (peMode == PathEndMode.ClosestTouch)
			{
				peMode = GenPath.ResolveClosestTouchPathMode(traveler, map, thing.Position);
			}
			switch (peMode)
			{
			case PathEndMode.None:
				return false;
			case PathEndMode.Touch:
				return true;
			case PathEndMode.OnCell:
				if (thing.def.size.x == 1 && thing.def.size.z == 1)
				{
					if (thing.Position.GetRegion(map) == region)
					{
						return true;
					}
				}
				else
				{
					foreach (IntVec3 item in thing.OccupiedRect())
					{
						if (item.GetRegion(map) == region)
						{
							return true;
						}
					}
				}
				return false;
			case PathEndMode.InteractionCell:
				if (thing.InteractionCell.GetRegion(map) == region)
				{
					return true;
				}
				return false;
			default:
				Log.Error("Unsupported PathEndMode: " + peMode);
				return false;
			}
		}
	}
}
