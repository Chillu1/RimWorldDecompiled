using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public static class Toils_JobTransforms
	{
		private static List<IntVec3> yieldedIngPlaceCells = new List<IntVec3>();

		public static Toil ExtractNextTargetFromQueue(TargetIndex ind, bool failIfCountFromQueueTooBig = true)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
				if (!targetQueue.NullOrEmpty())
				{
					if (failIfCountFromQueueTooBig && !curJob.countQueue.NullOrEmpty() && targetQueue[0].HasThing && curJob.countQueue[0] > targetQueue[0].Thing.stackCount)
					{
						actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
					}
					else
					{
						curJob.SetTarget(ind, targetQueue[0]);
						targetQueue.RemoveAt(0);
						if (!curJob.countQueue.NullOrEmpty())
						{
							curJob.count = curJob.countQueue[0];
							curJob.countQueue.RemoveAt(0);
						}
					}
				}
			};
			return toil;
		}

		public static Toil ClearQueue(TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				List<LocalTargetInfo> targetQueue = toil.actor.jobs.curJob.GetTargetQueue(ind);
				if (!targetQueue.NullOrEmpty())
				{
					targetQueue.Clear();
				}
			};
			return toil;
		}

		public static Toil ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex ind, Func<Thing, bool> validator = null)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				actor.jobs.curJob.GetTargetQueue(ind).RemoveAll((LocalTargetInfo ta) => !ta.HasThing || !ta.Thing.Spawned || ta.Thing.IsForbidden(actor) || (validator != null && !validator(ta.Thing)));
			};
			return toil;
		}

		private static IEnumerable<IntVec3> IngredientPlaceCellsInOrder(Thing destination)
		{
			yieldedIngPlaceCells.Clear();
			try
			{
				IntVec3 interactCell = destination.Position;
				IBillGiver billGiver = destination as IBillGiver;
				if (billGiver != null)
				{
					interactCell = ((Thing)billGiver).InteractionCell;
					foreach (IntVec3 item in billGiver.IngredientStackCells.OrderBy((IntVec3 c) => (c - interactCell).LengthHorizontalSquared))
					{
						yieldedIngPlaceCells.Add(item);
						yield return item;
					}
				}
				for (int i = 0; i < 200; i++)
				{
					IntVec3 intVec = interactCell + GenRadial.RadialPattern[i];
					if (!yieldedIngPlaceCells.Contains(intVec))
					{
						Building edifice = intVec.GetEdifice(destination.Map);
						if (edifice == null || edifice.def.passability != Traversability.Impassable || edifice.def.surfaceType != 0)
						{
							yield return intVec;
						}
					}
				}
			}
			finally
			{
				yieldedIngPlaceCells.Clear();
			}
		}

		public static Toil SetTargetToIngredientPlaceCell(TargetIndex facilityInd, TargetIndex carryItemInd, TargetIndex cellTargetInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(carryItemInd).Thing;
				IntVec3 c = IntVec3.Invalid;
				foreach (IntVec3 item in IngredientPlaceCellsInOrder(curJob.GetTarget(facilityInd).Thing))
				{
					if (!c.IsValid)
					{
						c = item;
					}
					bool flag = false;
					List<Thing> list = actor.Map.thingGrid.ThingsListAt(item);
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].def.category == ThingCategory.Item && (list[i].def != thing.def || list[i].stackCount == list[i].def.stackLimit))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						curJob.SetTarget(cellTargetInd, item);
						return;
					}
				}
				curJob.SetTarget(cellTargetInd, c);
			};
			return toil;
		}

		public static Toil MoveCurrentTargetIntoQueue(TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Job curJob = toil.actor.CurJob;
				LocalTargetInfo target = curJob.GetTarget(ind);
				if (target.IsValid)
				{
					List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
					if (targetQueue == null)
					{
						curJob.AddQueuedTarget(ind, target);
					}
					else
					{
						targetQueue.Insert(0, target);
					}
					curJob.SetTarget(ind, null);
				}
			};
			return toil;
		}

		public static Toil SucceedOnNoTargetInQueue(TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.EndOnNoTargetInQueue(ind, JobCondition.Succeeded);
			return toil;
		}
	}
}
