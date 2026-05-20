using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class Toils_Misc
{
	public static Toil Learn(SkillDef skill, float xp)
	{
		Toil toil = ToilMaker.MakeToil("Learn");
		toil.initAction = delegate
		{
			toil.actor.skills.Learn(skill, xp);
		};
		return toil;
	}

	public static Toil SetForbidden(TargetIndex ind, bool forbidden)
	{
		Toil toil = ToilMaker.MakeToil("SetForbidden");
		toil.initAction = delegate
		{
			toil.actor.CurJob.GetTarget(ind).Thing.SetForbidden(forbidden);
		};
		return toil;
	}

	public static Toil TakeItemFromInventoryToCarrier(Pawn pawn, TargetIndex itemInd)
	{
		Toil toil = ToilMaker.MakeToil("TakeItemFromInventoryToCarrier");
		toil.initAction = delegate
		{
			Job curJob = pawn.CurJob;
			Thing thing = (Thing)curJob.GetTarget(itemInd);
			int count = Mathf.Min(thing.stackCount, curJob.count);
			pawn.inventory.innerContainer.TryTransferToContainer(thing, pawn.carryTracker.innerContainer, count);
			curJob.SetTarget(itemInd, pawn.carryTracker.CarriedThing);
		};
		return toil;
	}

	public static Toil ThrowColonistAttackingMote(TargetIndex target)
	{
		Toil toil = ToilMaker.MakeToil("ThrowColonistAttackingMote");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Job curJob = actor.CurJob;
			if (actor.playerSettings != null && actor.playerSettings.UsesConfigurableHostilityResponse && !actor.Drafted && !actor.InMentalState && !curJob.playerForced && actor.HostileTo(curJob.GetTarget(target).Thing) && (!actor.IsMutant || !actor.mutant.Def.canBeDrafted))
			{
				MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistAttacking);
			}
		};
		return toil;
	}

	public static Toil FindRandomAdjacentReachableCell(TargetIndex adjacentToInd, TargetIndex cellInd)
	{
		Toil findCell = ToilMaker.MakeToil("FindRandomAdjacentReachableCell");
		findCell.initAction = delegate
		{
			Pawn actor = findCell.actor;
			Job curJob = actor.CurJob;
			LocalTargetInfo target = curJob.GetTarget(adjacentToInd);
			if (target.HasThing && (!target.Thing.Spawned || target.Thing.Map != actor.Map))
			{
				string obj = actor?.ToString();
				LocalTargetInfo localTargetInfo = target;
				Log.Error(obj + " could not find standable cell adjacent to " + localTargetInfo.ToString() + " because this thing is either unspawned or spawned somewhere else.");
				actor.jobs.curDriver.EndJobWith(JobCondition.Errored);
			}
			else
			{
				int num = 0;
				IntVec3 intVec;
				do
				{
					num++;
					if (num > 100)
					{
						string obj2 = actor?.ToString();
						LocalTargetInfo localTargetInfo = target;
						Log.Error(obj2 + " could not find standable cell adjacent to " + localTargetInfo.ToString());
						actor.jobs.curDriver.EndJobWith(JobCondition.Errored);
						return;
					}
					intVec = ((!target.HasThing) ? target.Cell.RandomAdjacentCell8Way() : target.Thing.RandomAdjacentCell8Way());
				}
				while (!intVec.Standable(actor.Map) || !actor.CanReserve(intVec) || !actor.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly));
				curJob.SetTarget(cellInd, intVec);
			}
		};
		return findCell;
	}
}
