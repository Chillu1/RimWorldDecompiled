using Verse;
using Verse.AI;

namespace RimWorld;

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
		Thing thing = GenClosest.ClosestThingReachable(GetRoot(pawn), pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.InteractionCell, TraverseParms.For(pawn), maxDistFromPoint, Validator);
		if (thing != null)
		{
			Job job = JobMaker.MakeJob(JobDefOf.ManTurret, thing);
			job.expiryInterval = 2000;
			job.checkOverrideOnExpire = true;
			return job;
		}
		return null;
		bool Validator(Thing t)
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
			if (JobDriver_ManTurret.FindAmmoForTurret(pawn, (Building_TurretGun)t) == null)
			{
				return false;
			}
			CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
			if (compRefuelable != null && !compRefuelable.HasFuel && JobDriver_ManTurret.FindFuelForTurret(pawn, (Building_TurretGun)t) == null)
			{
				return false;
			}
			return true;
		}
	}

	protected abstract IntVec3 GetRoot(Pawn pawn);
}
