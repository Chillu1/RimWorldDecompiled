using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobGiver_ManTurrets : ThinkNode_JobGiver
	{
		public float maxDistFromPoint = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_ManTurrets obj = (JobGiver_ManTurrets)base.DeepCopy(resolve);
			obj.maxDistFromPoint = maxDistFromPoint;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (!t.def.hasInteractionCell)
				{
					return false;
				}
				if (!t.def.HasComp(typeof(CompMannable)))
				{
					return false;
				}
				if (!pawn.CanReserve(t))
				{
					return false;
				}
				return (JobDriver_ManTurret.FindAmmoForTurret(pawn, (Building_TurretGun)t) != null) ? true : false;
			};
			Thing thing = GenClosest.ClosestThingReachable(GetRoot(pawn), pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.InteractionCell, TraverseParms.For(pawn), maxDistFromPoint, validator);
			if (thing != null)
			{
				Job job = JobMaker.MakeJob(JobDefOf.ManTurret, thing);
				job.expiryInterval = 2000;
				job.checkOverrideOnExpire = true;
				return job;
			}
			return null;
		}

		protected abstract IntVec3 GetRoot(Pawn pawn);
	}
}
