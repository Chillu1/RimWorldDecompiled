using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_RemovePaintBuilding : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.RemovePaintBuilding);
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.RemovePaintBuilding))
		{
			yield return item.target.Thing;
		}
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t.def.building == null || !t.def.building.paintable)
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.RemovePaintBuilding) == null)
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
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
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Job job = JobMaker.MakeJob(JobDefOf.RemovePaintBuilding);
		job.AddQueuedTarget(TargetIndex.A, t);
		for (int i = 0; i < 100; i++)
		{
			IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
			if (!intVec.InBounds(t.Map) || intVec.Fogged(t.Map) || !pawn.CanReach(intVec, PathEndMode.Touch, Danger.Deadly))
			{
				continue;
			}
			List<Thing> thingList = intVec.GetThingList(t.Map);
			for (int j = 0; j < thingList.Count; j++)
			{
				Thing thing = thingList[j];
				if (!job.targetQueueA.Contains(thing) && thing != t && HasJobOnThing(pawn, thing, forced))
				{
					job.AddQueuedTarget(TargetIndex.A, thing);
				}
			}
			if (job.GetTargetQueue(TargetIndex.A).Count >= 10)
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
}
