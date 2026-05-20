using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_DuelPickupWeapon : ThinkNode_JobGiver
{
	public int scanRadius = 3;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_DuelPickupWeapon obj = (JobGiver_DuelPickupWeapon)base.DeepCopy(resolve);
		obj.scanRadius = scanRadius;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.equipment.AllEquipmentListForReading.Any((ThingWithComps e) => e.def.IsMeleeWeapon))
		{
			return null;
		}
		if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return null;
		}
		LordJob_Ritual_Duel obj = (LordJob_Ritual_Duel)pawn.GetLord().LordJob;
		Pawn pawn2 = obj.Opponent(pawn);
		TargetInfo selectedTarget = obj.selectedTarget;
		Thing thing = null;
		float num = float.PositiveInfinity;
		for (int num2 = 0; num2 < GenRadial.NumCellsInRadius(scanRadius); num2++)
		{
			IntVec3 c = selectedTarget.Cell + GenRadial.RadialPattern[num2];
			if (!c.InBounds(selectedTarget.Map))
			{
				continue;
			}
			foreach (Thing thing2 in c.GetThingList(selectedTarget.Map))
			{
				float num3 = pawn.Position.DistanceTo(thing2.Position);
				if (thing2.def.IsMeleeWeapon && pawn.CanReserveAndReach(thing2, PathEndMode.ClosestTouch, Danger.Deadly) && EquipmentUtility.CanEquip(thing2, pawn) && num3 < pawn.Position.DistanceTo(pawn2.Position) && num3 < pawn2.Position.DistanceTo(thing2.Position) && num > num3)
				{
					thing = thing2;
					num = num3;
				}
			}
		}
		if (thing != null)
		{
			Job job = JobMaker.MakeJob(JobDefOf.Equip, thing);
			job.ignoreForbidden = true;
			return job;
		}
		return null;
	}
}
