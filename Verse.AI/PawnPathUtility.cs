using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public static class PawnPathUtility
{
	public static Thing FirstBlockingBuilding(this PawnPath path, out IntVec3 cellBefore, Pawn pawn)
	{
		if (!path.Found)
		{
			cellBefore = IntVec3.Invalid;
			return null;
		}
		List<IntVec3> nodesReversed = path.NodesReversed;
		if (nodesReversed.Count <= 1)
		{
			cellBefore = ((nodesReversed.Count == 1) ? nodesReversed[0] : IntVec3.Invalid);
			return null;
		}
		Building building = null;
		IntVec3 intVec = IntVec3.Invalid;
		for (int num = nodesReversed.Count - 2; num >= 0; num--)
		{
			Building edifice = nodesReversed[num].GetEdifice(pawn.Map);
			if (edifice != null)
			{
				bool num2 = edifice is Building_Door { FreePassage: false } building_Door && !building_Door.PawnCanOpen(pawn);
				bool flag = edifice.def.IsFence && !pawn.CanPassFences;
				if (num2 || flag || edifice.def.passability == Traversability.Impassable)
				{
					if (building != null)
					{
						cellBefore = intVec;
						return building;
					}
					cellBefore = nodesReversed[num + 1];
					return edifice;
				}
			}
			if (edifice != null && edifice.def.passability == Traversability.PassThroughOnly && edifice.def.Fillage == FillCategory.Full)
			{
				if (building == null)
				{
					building = edifice;
					intVec = nodesReversed[num + 1];
				}
			}
			else if (edifice == null || edifice.def.passability != Traversability.PassThroughOnly)
			{
				building = null;
			}
		}
		cellBefore = nodesReversed[0];
		return null;
	}

	public static bool TryFindLastCellBeforeBlockingDoor(this PawnPath path, Pawn pawn, out IntVec3 result)
	{
		if (path.NodesReversed.Count == 1)
		{
			result = path.NodesReversed[0];
			return false;
		}
		List<IntVec3> nodesReversed = path.NodesReversed;
		for (int num = nodesReversed.Count - 2; num >= 1; num--)
		{
			if (nodesReversed[num].GetEdifice(pawn.Map) is Building_Door building_Door && !building_Door.CanPhysicallyPass(pawn))
			{
				result = nodesReversed[num + 1];
				return true;
			}
		}
		result = nodesReversed[0];
		return false;
	}

	public static bool TryFindCellAtIndex(PawnPath path, int index, out IntVec3 result)
	{
		if (path.NodesReversed.Count <= index || index < 0)
		{
			result = IntVec3.Invalid;
			return false;
		}
		result = path.NodesReversed[path.NodesReversed.Count - 1 - index];
		return true;
	}
}
