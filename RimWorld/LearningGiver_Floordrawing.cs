using Verse;
using Verse.AI;

namespace RimWorld;

public class LearningGiver_Floordrawing : LearningGiver
{
	private static readonly IntVec3[] offsets = new IntVec3[2]
	{
		IntVec3.East,
		IntVec3.South
	};

	public static bool TryFindFloordrawingSpots(Pawn pawn, out IntVec3 drawFrom, out IntVec3 drawOn)
	{
		TraverseParms traverseParams = TraverseParms.For(pawn, Danger.None);
		IntVec3 innerDrawFrom = IntVec3.Invalid;
		IntVec3 innerDrawOn = IntVec3.Invalid;
		FindDrawCell(desperate: false);
		if (innerDrawFrom == IntVec3.Invalid)
		{
			FindDrawCell(desperate: true);
		}
		drawFrom = innerDrawFrom;
		drawOn = innerDrawOn;
		if (drawFrom != IntVec3.Invalid && drawOn != IntVec3.Invalid)
		{
			return true;
		}
		return false;
		void FindDrawCell(bool desperate)
		{
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region r) => r.Allows(traverseParams, isDestination: false), delegate(Region r)
			{
				if (r.IsForbiddenEntirely(pawn))
				{
					return false;
				}
				IntVec3 result;
				return r.DangerFor(pawn) != Danger.Deadly && r.TryFindRandomCellInRegion(CellValidator, out result);
			}, desperate ? 200 : 40);
			bool CellValidator(IntVec3 c)
			{
				if (!CanFloorDrawFrom(c, pawn))
				{
					return false;
				}
				bool canDrawOnRoot = CanDrawOn(c, pawn.Map, desperate);
				IntVec3[] array = offsets;
				foreach (IntVec3 intVec in array)
				{
					IntVec3 intVec2 = c + intVec;
					if (CanFloorDrawFrom(intVec2, pawn) && TryFloorDrawCellPair(intVec2, canDrawOnRoot, pawn, desperate, out var shouldDrawOnRoot))
					{
						innerDrawOn = (shouldDrawOnRoot ? c : intVec2);
						innerDrawFrom = (shouldDrawOnRoot ? intVec2 : c);
						return true;
					}
				}
				return false;
			}
		}
	}

	public static bool CanFloorDrawFrom(IntVec3 spot, Pawn drawer)
	{
		Map map = drawer.Map;
		if (spot.InBounds(map) && spot.Standable(map) && !spot.IsForbidden(drawer) && map.areaManager.Home[spot] && spot.GetDoor(map) == null)
		{
			return drawer.CanReserve(spot);
		}
		return false;
	}

	private static bool CanDrawOn(IntVec3 cell, Map map, bool desperate)
	{
		if (FilthMaker.CanMakeFilth(cell, map, ThingDefOf.Filth_Floordrawing, FilthSourceFlags.Pawn))
		{
			if (!desperate)
			{
				if (cell.GetFirstItem(map) == null && cell.GetFirstBuilding(map) == null)
				{
					return cell.GetFirstThing<Filth>(map) == null;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool TryFloorDrawCellPair(IntVec3 companionCell, bool canDrawOnRoot, Pawn drawer, bool desperate, out bool shouldDrawOnRoot)
	{
		shouldDrawOnRoot = false;
		if (!CanFloorDrawFrom(companionCell, drawer))
		{
			return false;
		}
		bool flag = CanDrawOn(companionCell, drawer.Map, desperate);
		if (!canDrawOnRoot && !flag)
		{
			return false;
		}
		if (!canDrawOnRoot)
		{
			return true;
		}
		if (!flag)
		{
			shouldDrawOnRoot = true;
			return true;
		}
		shouldDrawOnRoot = Rand.Bool;
		return true;
	}

	public override bool CanDo(Pawn pawn)
	{
		if (!base.CanDo(pawn))
		{
			return false;
		}
		IntVec3 drawFrom;
		IntVec3 drawOn;
		return TryFindFloordrawingSpots(pawn, out drawFrom, out drawOn);
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		if (!TryFindFloordrawingSpots(pawn, out var drawFrom, out var drawOn))
		{
			return null;
		}
		return JobMaker.MakeJob(def.jobDef, drawFrom, drawOn);
	}
}
