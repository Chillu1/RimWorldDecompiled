using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_RescueNearby : ThinkNode_JobGiver
	{
		private float radius = 30f;

		private const float MinDistFromEnemy = 25f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_RescueNearby obj = (JobGiver_RescueNearby)base.DeepCopy(resolve);
			obj.radius = radius;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				Pawn pawn3 = (Pawn)t;
				return (pawn3.Downed && pawn3.Faction == pawn.Faction && !pawn3.InBed() && pawn.CanReserve(pawn3) && !pawn3.IsForbidden(pawn) && !GenAI.EnemyIsNear(pawn3, 25f)) ? true : false;
			};
			Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn), radius, validator);
			if (pawn2 == null)
			{
				return null;
			}
			Building_Bed building_Bed = RestUtility.FindBedFor(pawn2, pawn, pawn2.HostFaction == pawn.Faction, checkSocialProperness: false);
			if (building_Bed == null || !pawn2.CanReserve(building_Bed))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.Rescue, pawn2, building_Bed);
			job.count = 1;
			return job;
		}
	}
}
