using RimWorld;

namespace Verse.AI;

public class JobGiver_PickupDroppedWeapon : ThinkNode_JobGiver
{
	public bool ignoreForbidden;

	protected override Job TryGiveJob(Pawn pawn)
	{
		Thing thing = pawn.mindState?.droppedWeapon;
		if (thing == null || thing.Destroyed || !thing.Spawned || thing.Map != pawn.Map)
		{
			return null;
		}
		return PickupWeaponJob(pawn, thing, ignoreForbidden);
	}

	public static Job PickupWeaponJob(Pawn pawn, Thing weapon, bool ignoreForbidden)
	{
		if (!pawn.CanReserveAndReach(weapon, PathEndMode.Touch, Danger.Deadly))
		{
			return null;
		}
		if (weapon.IsBurning())
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Equip, weapon);
		job.ignoreForbidden = ignoreForbidden;
		return job;
	}
}
