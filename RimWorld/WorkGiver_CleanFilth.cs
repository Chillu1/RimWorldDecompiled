using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_CleanFilth : WorkGiver_Scanner
{
	private const int MinTicksSinceThickened = 600;

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Filth);

	public override int MaxRegionsToScanBeforeGlobalSearch => 4;

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.listerFilthInHomeArea.FilthInHomeArea;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return pawn.Map.listerFilthInHomeArea.FilthInHomeArea.Count == 0;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Filth filth))
		{
			return false;
		}
		if (!filth.Map.areaManager.Home[filth.Position])
		{
			return false;
		}
		if (filth.Fogged() || !pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (filth.TicksSinceThickened < 600)
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Clean);
		job.AddQueuedTarget(TargetIndex.A, t);
		int num = 15;
		Map map = t.Map;
		Room room = t.GetRoom();
		for (int i = 0; i < 100; i++)
		{
			IntVec3 c = t.Position + GenRadial.RadialPattern[i];
			if (!ShouldClean(c))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int j = 0; j < thingList.Count; j++)
			{
				Thing thing = thingList[j];
				if (HasJobOnThing(pawn, thing, forced) && thing != t)
				{
					job.AddQueuedTarget(TargetIndex.A, thing);
				}
			}
			if (job.GetTargetQueue(TargetIndex.A).Count >= num)
			{
				break;
			}
		}
		if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
		{
			job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
		}
		return job;
		bool ShouldClean(IntVec3 intVec)
		{
			if (!intVec.InBounds(map))
			{
				return false;
			}
			Room room2 = intVec.GetRoom(map);
			if (room == room2)
			{
				return true;
			}
			Region region = intVec.GetDoor(map)?.GetRegion(RegionType.Portal);
			if (region != null && !region.links.NullOrEmpty())
			{
				for (int k = 0; k < region.links.Count; k++)
				{
					RegionLink regionLink = region.links[k];
					for (int l = 0; l < 2; l++)
					{
						if (regionLink.regions[l] != null && regionLink.regions[l] != region && regionLink.regions[l].valid && regionLink.regions[l].Room == room)
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
