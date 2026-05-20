using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_TakeCountToInventory : ThinkNode_JobGiver
	{
		public ThingDef def;

		public int count = 1;

		protected override Job TryGiveJob(Pawn pawn)
		{
			int toTake = Math.Max(count - pawn.inventory.Count(def), 0);
			if (toTake == 0)
			{
				return null;
			}
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(def), PathEndMode.Touch, TraverseParms.For(pawn), 9999f, (Thing x) => x.stackCount >= toTake && !x.IsForbidden(pawn) && pawn.CanReserve(x, 10, toTake));
			if (thing != null)
			{
				Job job = JobMaker.MakeJob(JobDefOf.TakeCountToInventory, thing);
				job.count = toTake;
				return job;
			}
			return null;
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_TakeCountToInventory obj = (JobGiver_TakeCountToInventory)base.DeepCopy(resolve);
			obj.def = def;
			obj.count = count;
			return obj;
		}
	}
}
