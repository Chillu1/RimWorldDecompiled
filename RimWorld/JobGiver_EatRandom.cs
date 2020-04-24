using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_EatRandom : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.Downed)
			{
				return null;
			}
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (t.def.category != ThingCategory.Item)
				{
					return false;
				}
				if (!t.IngestibleNow)
				{
					return false;
				}
				if (!pawn.RaceProps.CanEverEat(t))
				{
					return false;
				}
				return pawn.CanReserve(t) ? true : false;
			};
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.OnCell, TraverseParms.For(pawn), 10f, validator);
			if (thing == null)
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Ingest, thing);
		}
	}
}
