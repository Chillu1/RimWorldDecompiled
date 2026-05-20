using System.Collections.Generic;
using Verse.AI;

namespace Verse;

public static class ReachabilityUtility
{
	public static bool CanReach(this Pawn pawn, LocalTargetInfo dest, PathEndMode peMode, Danger maxDanger, bool canBashDoors = false, bool canBashFences = false, TraverseMode mode = TraverseMode.ByPawn)
	{
		if (!pawn.Spawned)
		{
			return false;
		}
		return pawn.Map.reachability.CanReach(pawn.Position, dest, peMode, TraverseParms.For(pawn, maxDanger, mode, canBashDoors, alwaysUseAvoidGrid: false, canBashFences));
	}

	public static bool CanReach(this Pawn pawn, IntVec3 start, LocalTargetInfo dest, PathEndMode peMode, Danger maxDanger, bool canBashDoors = false, bool canBashFences = false, TraverseMode mode = TraverseMode.ByPawn)
	{
		if (!pawn.Spawned)
		{
			return false;
		}
		return pawn.Map.reachability.CanReach(start, dest, peMode, TraverseParms.For(pawn, maxDanger, mode, canBashDoors, alwaysUseAvoidGrid: false, canBashFences));
	}

	public static bool CanReachNonLocal(this Pawn pawn, TargetInfo dest, PathEndMode peMode, Danger maxDanger, bool canBashDoors = false, TraverseMode mode = TraverseMode.ByPawn)
	{
		if (!pawn.Spawned)
		{
			return false;
		}
		return pawn.Map.reachability.CanReachNonLocal(pawn.Position, dest, peMode, TraverseParms.For(pawn, maxDanger, mode, canBashDoors));
	}

	public static bool CanReachMapEdge(this Pawn p)
	{
		if (!p.Spawned)
		{
			return false;
		}
		return p.Map.reachability.CanReachMapEdge(p.Position, TraverseParms.For(p));
	}

	public static void ClearCache()
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			maps[i].reachability.ClearCache();
		}
	}

	public static void ClearCacheFor(Pawn p)
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			maps[i].reachability.ClearCacheFor(p);
		}
	}
}
