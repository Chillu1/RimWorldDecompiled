using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_Misc
	{
		public static Toil Learn(SkillDef skill, float xp)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.skills.Learn(skill, xp);
			};
			return toil;
		}

		public static Toil SetForbidden(TargetIndex ind, bool forbidden)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.CurJob.GetTarget(ind).Thing.SetForbidden(forbidden);
			};
			return toil;
		}

		public static Toil TakeItemFromInventoryToCarrier(Pawn pawn, TargetIndex itemInd)
		{
			return new Toil
			{
				initAction = delegate
				{
					Job curJob = pawn.CurJob;
					Thing thing = (Thing)curJob.GetTarget(itemInd);
					int count = Mathf.Min(thing.stackCount, curJob.count);
					pawn.inventory.innerContainer.TryTransferToContainer(thing, pawn.carryTracker.innerContainer, count);
					curJob.SetTarget(itemInd, pawn.carryTracker.CarriedThing);
				}
			};
		}

		public static Toil ThrowColonistAttackingMote(TargetIndex target)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.CurJob;
				if (actor.playerSettings != null && actor.playerSettings.UsesConfigurableHostilityResponse && !actor.Drafted && !actor.InMentalState && !curJob.playerForced && actor.HostileTo(curJob.GetTarget(target).Thing))
				{
					MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistAttacking);
				}
			};
			return toil;
		}

		public static Toil FindRandomAdjacentReachableCell(TargetIndex adjacentToInd, TargetIndex cellInd)
		{
			Toil findCell = new Toil();
			findCell.initAction = delegate
			{
				Pawn actor = findCell.actor;
				Job curJob = actor.CurJob;
				LocalTargetInfo target = curJob.GetTarget(adjacentToInd);
				if (target.HasThing && (!target.Thing.Spawned || target.Thing.Map != actor.Map))
				{
					Log.Error(string.Concat(actor, " could not find standable cell adjacent to ", target, " because this thing is either unspawned or spawned somewhere else."));
					actor.jobs.curDriver.EndJobWith(JobCondition.Errored);
				}
				else
				{
					int num = 0;
					IntVec3 c;
					do
					{
						num++;
						if (num > 100)
						{
							Log.Error(string.Concat(actor, " could not find standable cell adjacent to ", target));
							actor.jobs.curDriver.EndJobWith(JobCondition.Errored);
							return;
						}
						c = ((!target.HasThing) ? target.Cell.RandomAdjacentCell8Way() : target.Thing.RandomAdjacentCell8Way());
					}
					while (!c.Standable(actor.Map) || !actor.CanReserve(c) || !actor.CanReach(c, PathEndMode.OnCell, Danger.Deadly));
					curJob.SetTarget(cellInd, c);
				}
			};
			return findCell;
		}
	}
}
