using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_BuildSnowman : JoyGiver
{
	private const float MinSnowmanDepth = 0.5f;

	private const float MinDistBetweenSnowmen = 12f;

	private const float MinSnowDepth = 200f;

	public override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Construction))
		{
			return null;
		}
		if (!JoyUtility.EnjoyableOutsideNow(pawn))
		{
			return null;
		}
		if (pawn.Map.snowGrid.TotalDepth < 200f)
		{
			return null;
		}
		IntVec3 intVec = TryFindSnowmanBuildCell(pawn);
		if (!intVec.IsValid)
		{
			return null;
		}
		return JobMaker.MakeJob(def.jobDef, intVec);
	}

	private static IntVec3 TryFindSnowmanBuildCell(Pawn pawn)
	{
		if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(), TraverseParms.For(pawn), (Region r) => r.Room.PsychologicallyOutdoors, 100, out var rootReg))
		{
			return IntVec3.Invalid;
		}
		IntVec3 result = IntVec3.Invalid;
		RegionTraverser.BreadthFirstTraverse(rootReg, (Region from, Region region) => region.District == rootReg.District, delegate(Region region)
		{
			for (int i = 0; i < 5; i++)
			{
				IntVec3 randomCell = region.RandomCell;
				if (IsGoodSnowmanCell(randomCell, pawn))
				{
					result = randomCell;
					return true;
				}
			}
			return false;
		}, 30);
		return result;
	}

	private static bool IsGoodSnowmanCell(IntVec3 c, Pawn pawn)
	{
		if (pawn.Map.snowGrid.GetDepth(c) < 0.5f)
		{
			return false;
		}
		if (c.Fogged(pawn.Map))
		{
			return false;
		}
		if (c.GetEdifice(pawn.Map) != null)
		{
			return false;
		}
		if (c.IsForbidden(pawn))
		{
			return false;
		}
		for (int i = 0; i < 9; i++)
		{
			IntVec3 intVec = c + GenAdj.AdjacentCellsAndInside[i];
			if (!intVec.InBounds(pawn.Map))
			{
				return false;
			}
			if (!intVec.Standable(pawn.Map))
			{
				return false;
			}
			if (pawn.Map.reservationManager.IsReservedAndRespected(intVec, pawn))
			{
				return false;
			}
		}
		List<Thing> list = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Snowman);
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].Position.InHorDistOf(c, 12f))
			{
				return false;
			}
		}
		return true;
	}
}
