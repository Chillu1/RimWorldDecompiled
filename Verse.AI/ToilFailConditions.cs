using RimWorld;
using System;

namespace Verse.AI
{
	public static class ToilFailConditions
	{
		public static Toil FailOn(this Toil toil, Func<Toil, bool> condition)
		{
			toil.AddEndCondition(() => (!condition(toil)) ? JobCondition.Ongoing : JobCondition.Incompletable);
			return toil;
		}

		public static T FailOn<T>(this T f, Func<bool> condition) where T : IJobEndable
		{
			f.AddEndCondition(() => (!condition()) ? JobCondition.Ongoing : JobCondition.Incompletable);
			return f;
		}

		public static T FailOnDestroyedOrNull<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(() => (!f.GetActor().jobs.curJob.GetTarget(ind).Thing.DestroyedOrNull()) ? JobCondition.Ongoing : JobCondition.Incompletable);
			return f;
		}

		public static T FailOnDespawnedOrNull<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				LocalTargetInfo target = f.GetActor().jobs.curJob.GetTarget(ind);
				Thing thing = target.Thing;
				if (thing == null && target.IsValid)
				{
					return JobCondition.Ongoing;
				}
				return (thing != null && thing.Spawned && thing.Map == f.GetActor().Map) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			return f;
		}

		public static T EndOnDespawnedOrNull<T>(this T f, TargetIndex ind, JobCondition endCondition = JobCondition.Incompletable) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				LocalTargetInfo target = f.GetActor().jobs.curJob.GetTarget(ind);
				Thing thing = target.Thing;
				if (thing == null && target.IsValid)
				{
					return JobCondition.Ongoing;
				}
				return (thing != null && thing.Spawned && thing.Map == f.GetActor().Map) ? JobCondition.Ongoing : endCondition;
			});
			return f;
		}

		public static T EndOnNoTargetInQueue<T>(this T f, TargetIndex ind, JobCondition endCondition = JobCondition.Incompletable) where T : IJobEndable
		{
			f.AddEndCondition(() => (!f.GetActor().jobs.curJob.GetTargetQueue(ind).NullOrEmpty()) ? JobCondition.Ongoing : endCondition);
			return f;
		}

		public static T FailOnDowned<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(() => (!((Pawn)f.GetActor().jobs.curJob.GetTarget(ind).Thing).Downed) ? JobCondition.Ongoing : JobCondition.Incompletable);
			return f;
		}

		public static T FailOnDownedOrDead<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Thing thing = f.GetActor().jobs.curJob.GetTarget(ind).Thing;
				return (!((Pawn)thing).Downed && !((Pawn)thing).Dead) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			return f;
		}

		public static T FailOnMobile<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(() => (((Pawn)f.GetActor().jobs.curJob.GetTarget(ind).Thing).health.State != PawnHealthState.Mobile) ? JobCondition.Ongoing : JobCondition.Incompletable);
			return f;
		}

		public static T FailOnNotDowned<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(() => ((Pawn)f.GetActor().jobs.curJob.GetTarget(ind).Thing).Downed ? JobCondition.Ongoing : JobCondition.Incompletable);
			return f;
		}

		public static T FailOnNotAwake<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(() => ((Pawn)f.GetActor().jobs.curJob.GetTarget(ind).Thing).Awake() ? JobCondition.Ongoing : JobCondition.Incompletable);
			return f;
		}

		public static T FailOnNotCasualInterruptible<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(() => ((Pawn)f.GetActor().jobs.curJob.GetTarget(ind).Thing).CanCasuallyInteractNow() ? JobCondition.Ongoing : JobCondition.Incompletable);
			return f;
		}

		public static T FailOnMentalState<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn pawn = f.GetActor().jobs.curJob.GetTarget(ind).Thing as Pawn;
				return (pawn == null || !pawn.InMentalState) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			return f;
		}

		public static T FailOnAggroMentalState<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn pawn = f.GetActor().jobs.curJob.GetTarget(ind).Thing as Pawn;
				return (pawn == null || !pawn.InAggroMentalState) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			return f;
		}

		public static T FailOnAggroMentalStateAndHostile<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn pawn = f.GetActor().jobs.curJob.GetTarget(ind).Thing as Pawn;
				return (pawn == null || !pawn.InAggroMentalState || !pawn.HostileTo(f.GetActor())) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			return f;
		}

		public static T FailOnSomeonePhysicallyInteracting<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				Thing thing = actor.jobs.curJob.GetTarget(ind).Thing;
				return (thing == null || !actor.Map.physicalInteractionReservationManager.IsReserved(thing) || actor.Map.physicalInteractionReservationManager.IsReservedBy(actor, thing)) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			return f;
		}

		public static T FailOnForbidden<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				if (actor.Faction != Faction.OfPlayer)
				{
					return JobCondition.Ongoing;
				}
				if (actor.jobs.curJob.ignoreForbidden)
				{
					return JobCondition.Ongoing;
				}
				Thing thing = actor.jobs.curJob.GetTarget(ind).Thing;
				if (thing == null)
				{
					return JobCondition.Ongoing;
				}
				return (!thing.IsForbidden(actor)) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			return f;
		}

		public static T FailOnDespawnedNullOrForbidden<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.FailOnDespawnedOrNull(ind);
			f.FailOnForbidden(ind);
			return f;
		}

		public static T FailOnDestroyedNullOrForbidden<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.FailOnDestroyedOrNull(ind);
			f.FailOnForbidden(ind);
			return f;
		}

		public static T FailOnThingMissingDesignation<T>(this T f, TargetIndex ind, DesignationDef desDef) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				Job curJob = actor.jobs.curJob;
				if (curJob.ignoreDesignations)
				{
					return JobCondition.Ongoing;
				}
				Thing thing = curJob.GetTarget(ind).Thing;
				return (thing != null && actor.Map.designationManager.DesignationOn(thing, desDef) != null) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			return f;
		}

		public static T FailOnThingHavingDesignation<T>(this T f, TargetIndex ind, DesignationDef desDef) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				Job curJob = actor.jobs.curJob;
				if (curJob.ignoreDesignations)
				{
					return JobCondition.Ongoing;
				}
				Thing thing = curJob.GetTarget(ind).Thing;
				return (thing != null && actor.Map.designationManager.DesignationOn(thing, desDef) == null) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			return f;
		}

		public static T FailOnCellMissingDesignation<T>(this T f, TargetIndex ind, DesignationDef desDef) where T : IJobEndable
		{
			f.AddEndCondition(delegate
			{
				Pawn actor = f.GetActor();
				Job curJob = actor.jobs.curJob;
				if (curJob.ignoreDesignations)
				{
					return JobCondition.Ongoing;
				}
				return (actor.Map.designationManager.DesignationAt(curJob.GetTarget(ind).Cell, desDef) != null) ? JobCondition.Ongoing : JobCondition.Incompletable;
			});
			return f;
		}

		public static T FailOnBurningImmobile<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			f.AddEndCondition(() => (!f.GetActor().jobs.curJob.GetTarget(ind).ToTargetInfo(f.GetActor().Map).IsBurning()) ? JobCondition.Ongoing : JobCondition.Incompletable);
			return f;
		}

		public static T FailOnCannotTouch<T>(this T f, TargetIndex ind, PathEndMode peMode) where T : IJobEndable
		{
			f.AddEndCondition(() => f.GetActor().CanReachImmediate(f.GetActor().jobs.curJob.GetTarget(ind), peMode) ? JobCondition.Ongoing : JobCondition.Incompletable);
			return f;
		}

		public static T FailOnIncapable<T>(this T f, PawnCapacityDef pawnCapacity) where T : IJobEndable
		{
			f.AddEndCondition(() => f.GetActor().health.capacities.CapableOf(pawnCapacity) ? JobCondition.Ongoing : JobCondition.Incompletable);
			return f;
		}

		public static Toil FailOnDespawnedNullOrForbiddenPlacedThings(this Toil toil)
		{
			toil.AddFailCondition(delegate
			{
				if (toil.actor.jobs.curJob.placedThings == null)
				{
					return false;
				}
				for (int i = 0; i < toil.actor.jobs.curJob.placedThings.Count; i++)
				{
					ThingCountClass thingCountClass = toil.actor.jobs.curJob.placedThings[i];
					if (thingCountClass.thing == null || !thingCountClass.thing.Spawned || thingCountClass.thing.Map != toil.actor.Map || (!toil.actor.CurJob.ignoreForbidden && thingCountClass.thing.IsForbidden(toil.actor)))
					{
						return true;
					}
				}
				return false;
			});
			return toil;
		}
	}
}
