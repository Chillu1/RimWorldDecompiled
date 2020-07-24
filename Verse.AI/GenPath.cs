using RimWorld;

namespace Verse.AI
{
	public static class GenPath
	{
		public static TargetInfo ResolvePathMode(Pawn pawn, TargetInfo dest, ref PathEndMode peMode)
		{
			if (dest.HasThing && !dest.Thing.Spawned)
			{
				peMode = PathEndMode.Touch;
				return dest;
			}
			if (peMode == PathEndMode.InteractionCell)
			{
				if (!dest.HasThing)
				{
					Log.Error(string.Concat("Pathed to cell ", dest, " with PathEndMode.InteractionCell."));
				}
				peMode = PathEndMode.OnCell;
				return new TargetInfo(dest.Thing.InteractionCell, dest.Thing.Map);
			}
			if (peMode == PathEndMode.ClosestTouch)
			{
				peMode = ResolveClosestTouchPathMode(pawn, dest.Map, dest.Cell);
			}
			return dest;
		}

		public static PathEndMode ResolveClosestTouchPathMode(Pawn pawn, Map map, IntVec3 target)
		{
			if (ShouldNotEnterCell(pawn, map, target))
			{
				return PathEndMode.Touch;
			}
			return PathEndMode.OnCell;
		}

		private static bool ShouldNotEnterCell(Pawn pawn, Map map, IntVec3 dest)
		{
			if (map.pathGrid.PerceivedPathCostAt(dest) > 30)
			{
				return true;
			}
			if (!dest.Walkable(map))
			{
				return true;
			}
			if (pawn != null)
			{
				if (dest.IsForbidden(pawn))
				{
					return true;
				}
				Building edifice = dest.GetEdifice(map);
				if (edifice != null)
				{
					Building_Door building_Door = edifice as Building_Door;
					if (building_Door != null)
					{
						if (building_Door.IsForbidden(pawn))
						{
							return true;
						}
						if (!building_Door.PawnCanOpen(pawn))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
