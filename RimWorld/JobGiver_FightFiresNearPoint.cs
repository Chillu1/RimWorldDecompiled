using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	internal class JobGiver_FightFiresNearPoint : ThinkNode_JobGiver
	{
		public float maxDistFromPoint = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_FightFiresNearPoint obj = (JobGiver_FightFiresNearPoint)base.DeepCopy(resolve);
			obj.maxDistFromPoint = maxDistFromPoint;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (((AttachableThing)t).parent is Pawn)
				{
					return false;
				}
				if (!pawn.CanReserve(t))
				{
					return false;
				}
				return (!pawn.WorkTagIsDisabled(WorkTags.Firefighting)) ? true : false;
			};
			Thing thing = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map, ThingRequest.ForDef(ThingDefOf.Fire), PathEndMode.Touch, TraverseParms.For(pawn), maxDistFromPoint, validator);
			if (thing != null)
			{
				return JobMaker.MakeJob(JobDefOf.BeatFire, thing);
			}
			return null;
		}
	}
}
