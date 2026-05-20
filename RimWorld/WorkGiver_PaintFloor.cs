using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_PaintFloor : WorkGiver_Scanner
{
	private static List<Thing> tmpDye = new List<Thing>();

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.PaintFloor);
	}

	public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
	{
		foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.PaintFloor))
		{
			yield return item.target.Cell;
		}
	}

	public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
	{
		AcceptanceReport acceptanceReport = ShouldPaintCell(pawn, c, forced, checkDye: true);
		if (!acceptanceReport)
		{
			if (!acceptanceReport.Reason.NullOrEmpty())
			{
				JobFailReason.Is(acceptanceReport.Reason);
			}
			return false;
		}
		return true;
	}

	public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
	{
		tmpDye.Clear();
		tmpDye = PaintUtility.FindNearbyDyes(pawn, forced);
		int stackCountFromThingList = ThingUtility.GetStackCountFromThingList(tmpDye);
		if (!tmpDye.Any())
		{
			return null;
		}
		tmpDye.SortBy((Thing x) => x.Position.DistanceToSquared(cell));
		int num = 0;
		Job job = JobMaker.MakeJob(JobDefOf.PaintFloor);
		job.AddQueuedTarget(TargetIndex.A, cell);
		job.AddQueuedTarget(TargetIndex.B, tmpDye[num]);
		job.countQueue = new List<int> { 1 };
		int num2 = Mathf.Min(10, stackCountFromThingList);
		for (int num3 = 0; num3 < 100; num3++)
		{
			IntVec3 intVec = cell + GenRadial.RadialPattern[num3];
			if (!intVec.InBounds(pawn.Map) || intVec.Fogged(pawn.Map) || !pawn.CanReach(intVec, PathEndMode.Touch, Danger.Deadly))
			{
				continue;
			}
			if ((bool)ShouldPaintCell(pawn, intVec, forced, checkDye: false))
			{
				if (job.targetQueueA.Contains(intVec))
				{
					continue;
				}
				job.AddQueuedTarget(TargetIndex.A, intVec);
				job.countQueue[0]++;
				if (job.countQueue[0] >= tmpDye[num].stackCount)
				{
					num++;
					if (num >= tmpDye.Count)
					{
						break;
					}
					job.AddQueuedTarget(TargetIndex.B, tmpDye[num]);
				}
			}
			if (job.GetTargetQueue(TargetIndex.A).Count >= num2)
			{
				break;
			}
		}
		if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
		{
			job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
		}
		return job;
	}

	private AcceptanceReport ShouldPaintCell(Pawn pawn, IntVec3 c, bool forced, bool checkDye)
	{
		if (!pawn.Map.terrainGrid.TerrainAt(c).isPaintable)
		{
			return false;
		}
		Designation designation = pawn.Map.designationManager.DesignationAt(c, DesignationDefOf.PaintFloor);
		if (designation?.colorDef == null)
		{
			return false;
		}
		if (pawn.Map.terrainGrid.ColorAt(c) == designation.colorDef)
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationAt(c, DesignationDefOf.PaintFloor) == null)
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationAt(c, DesignationDefOf.RemoveFloor) != null)
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationAt(c, DesignationDefOf.RemovePaintFloor) != null)
		{
			return false;
		}
		if (!pawn.CanReserve(c, 1, -1, ReservationLayerDefOf.Floor, forced))
		{
			return false;
		}
		if (checkDye)
		{
			List<Thing> list = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Dye);
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].IsForbidden(pawn) && pawn.CanReserveAndReach(list[i], PathEndMode.ClosestTouch, Danger.Deadly, 1, 1, null, forced))
				{
					return true;
				}
			}
			return "NoIngredient".Translate(ThingDefOf.Dye);
		}
		return true;
	}
}
