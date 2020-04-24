using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	internal class WorkGiver_CleanFilth : WorkGiver_Scanner
	{
		private int MinTicksSinceThickened = 600;

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
			Filth filth = t as Filth;
			if (filth == null)
			{
				return false;
			}
			if (!filth.Map.areaManager.Home[filth.Position])
			{
				return false;
			}
			if (!pawn.CanReserve(t, 1, -1, null, forced))
			{
				return false;
			}
			if (filth.TicksSinceThickened < MinTicksSinceThickened)
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
				IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(map) || intVec.GetRoom(map) != room)
				{
					continue;
				}
				List<Thing> thingList = intVec.GetThingList(map);
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
		}
	}
}
