using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_PaintBuilding : WorkGiver_Scanner
{
	private static List<Thing> tmpDye = new List<Thing>();

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.PaintBuilding);
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.PaintBuilding))
		{
			yield return item.target.Thing;
		}
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		AcceptanceReport acceptanceReport = ShouldPaintThing(pawn, t, forced, checkDye: true);
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

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		tmpDye.Clear();
		tmpDye = PaintUtility.FindNearbyDyes(pawn, forced);
		int stackCountFromThingList = ThingUtility.GetStackCountFromThingList(tmpDye);
		if (!tmpDye.Any())
		{
			return null;
		}
		tmpDye.SortBy((Thing x) => x.Position.DistanceToSquared(t.Position));
		int num = 0;
		Job job = JobMaker.MakeJob(JobDefOf.PaintBuilding);
		job.AddQueuedTarget(TargetIndex.A, t);
		job.AddQueuedTarget(TargetIndex.B, tmpDye[num]);
		job.countQueue = new List<int> { 1 };
		int num2 = Mathf.Min(10, stackCountFromThingList);
		for (int num3 = 0; num3 < 100; num3++)
		{
			IntVec3 intVec = t.Position + GenRadial.RadialPattern[num3];
			if (!intVec.InBounds(t.Map) || intVec.Fogged(t.Map) || !pawn.CanReach(intVec, PathEndMode.Touch, Danger.Deadly))
			{
				continue;
			}
			List<Thing> thingList = intVec.GetThingList(t.Map);
			for (int num4 = 0; num4 < thingList.Count; num4++)
			{
				Thing thing = thingList[num4];
				if (thing == t || !ShouldPaintThing(pawn, thing, forced, checkDye: false) || job.targetQueueA.Contains(thing))
				{
					continue;
				}
				job.AddQueuedTarget(TargetIndex.A, thing);
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
			if (job.GetTargetQueue(TargetIndex.A).Count >= num2 || num >= tmpDye.Count)
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

	private AcceptanceReport ShouldPaintThing(Pawn pawn, Thing t, bool forced, bool checkDye)
	{
		if (t.def.building == null || !t.def.building.paintable)
		{
			return false;
		}
		Designation designation = pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.PaintBuilding);
		if (designation?.colorDef == null)
		{
			return false;
		}
		if (t is Building building && building.PaintColorDef == designation.colorDef)
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.PaintBuilding) == null)
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.RemovePaintBuilding) != null)
		{
			return false;
		}
		if (t.IsBurning())
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
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
