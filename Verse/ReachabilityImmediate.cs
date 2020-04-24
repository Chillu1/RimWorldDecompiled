using Verse.AI;

namespace Verse
{
	public static class ReachabilityImmediate
	{
		public static bool CanReachImmediate(IntVec3 start, LocalTargetInfo target, Map map, PathEndMode peMode, Pawn pawn)
		{
			if (!target.IsValid)
			{
				return false;
			}
			target = (LocalTargetInfo)GenPath.ResolvePathMode(pawn, target.ToTargetInfo(map), ref peMode);
			if (target.HasThing)
			{
				Thing thing = target.Thing;
				if (!thing.Spawned)
				{
					if (pawn != null)
					{
						if (pawn.carryTracker.innerContainer.Contains(thing))
						{
							return true;
						}
						if (pawn.inventory.innerContainer.Contains(thing))
						{
							return true;
						}
						if (pawn.apparel != null && pawn.apparel.Contains(thing))
						{
							return true;
						}
						if (pawn.equipment != null && pawn.equipment.Contains(thing))
						{
							return true;
						}
					}
					return false;
				}
				if (thing.Map != map)
				{
					return false;
				}
			}
			if (!target.HasThing || (target.Thing.def.size.x == 1 && target.Thing.def.size.z == 1))
			{
				if (start == target.Cell)
				{
					return true;
				}
			}
			else if (start.IsInside(target.Thing))
			{
				return true;
			}
			if (peMode == PathEndMode.Touch && TouchPathEndModeUtility.IsAdjacentOrInsideAndAllowedToTouch(start, target, map))
			{
				return true;
			}
			return false;
		}

		public static bool CanReachImmediate(this Pawn pawn, LocalTargetInfo target, PathEndMode peMode)
		{
			if (!pawn.Spawned)
			{
				return false;
			}
			return CanReachImmediate(pawn.Position, target, pawn.Map, peMode, pawn);
		}

		public static bool CanReachImmediateNonLocal(this Pawn pawn, TargetInfo target, PathEndMode peMode)
		{
			if (!pawn.Spawned)
			{
				return false;
			}
			if (target.Map != null && target.Map != pawn.Map)
			{
				return false;
			}
			return pawn.CanReachImmediate((LocalTargetInfo)target, peMode);
		}

		public static bool CanReachImmediate(IntVec3 start, CellRect rect, Map map, PathEndMode peMode, Pawn pawn)
		{
			IntVec3 c = rect.ClosestCellTo(start);
			return CanReachImmediate(start, c, map, peMode, pawn);
		}
	}
}
