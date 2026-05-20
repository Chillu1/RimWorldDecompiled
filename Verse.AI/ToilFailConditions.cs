using System;
using RimWorld;

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
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => (!f.GetActor().jobs.curJob.GetTarget(ind).Thing.DestroyedOrNull()) ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnInvalidOrDestroyed<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = delegate
			{
				LocalTargetInfo target = f.GetActor().jobs.curJob.GetTarget(ind);
				return (target.IsValid && !target.ThingDestroyed) ? JobCondition.Ongoing : JobCondition.Incompletable;
			};
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static bool DespawnedOrNull(LocalTargetInfo target, Pawn actor)
		{
			Thing thing = target.Thing;
			if (thing == null && target.IsValid)
			{
				return false;
			}
			if (thing == null || !thing.Spawned || thing.Map != actor.Map)
			{
				return true;
			}
			return false;
		}

		public static T FailOnDespawnedOrNull<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => (!DespawnedOrNull(f.GetActor().jobs.curJob.GetTarget(ind), f.GetActor())) ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static bool SelfAndParentsDespawnedOrNull(LocalTargetInfo target, Pawn actor)
		{
			Thing thing = target.Thing;
			if (thing == null && target.IsValid)
			{
				return false;
			}
			if (thing == null || !thing.SpawnedOrAnyParentSpawned || thing.MapHeld != actor.Map)
			{
				return true;
			}
			return false;
		}

		public static T FailOnSelfAndParentsDespawnedOrNull<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => (!SelfAndParentsDespawnedOrNull(f.GetActor().jobs.curJob.GetTarget(ind), f.GetActor())) ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T EndOnDespawnedOrNull<T>(this T f, TargetIndex ind, JobCondition endCondition = JobCondition.Incompletable) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => (!DespawnedOrNull(f.GetActor().jobs.curJob.GetTarget(ind), f.GetActor())) ? JobCondition.Ongoing : endCondition;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T EndOnNoTargetInQueue<T>(this T f, TargetIndex ind, JobCondition endCondition = JobCondition.Incompletable) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => (!f.GetActor().jobs.curJob.GetTargetQueue(ind).NullOrEmpty()) ? JobCondition.Ongoing : endCondition;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnDowned<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => (!((Pawn)f.GetActor().jobs.curJob.GetTarget(ind).Thing).Downed) ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnMobile<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => (((Pawn)f.GetActor().jobs.curJob.GetTarget(ind).Thing).health.State != PawnHealthState.Mobile) ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnNotDowned<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => ((Pawn)f.GetActor().jobs.curJob.GetTarget(ind).Thing).Downed ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnNotAwake<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => ((Pawn)f.GetActor().jobs.curJob.GetTarget(ind).Thing).Awake() ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnNotCasualInterruptible<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => ((Pawn)f.GetActor().jobs.curJob.GetTarget(ind).Thing).CanCasuallyInteractNow() ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnMentalState<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => (!(f.GetActor().jobs.curJob.GetTarget(ind).Thing is Pawn { InMentalState: not false } pawn) || pawn.health.hediffSet.HasHediff(HediffDefOf.Scaria)) ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnAggroMentalState<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => (!(f.GetActor().jobs.curJob.GetTarget(ind).Thing is Pawn { InAggroMentalState: not false } pawn) || pawn.health.hediffSet.HasHediff(HediffDefOf.Scaria)) ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnAggroMentalStateAndHostile<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => (!(f.GetActor().jobs.curJob.GetTarget(ind).Thing is Pawn { InAggroMentalState: not false } pawn) || pawn.health.hediffSet.HasHediff(HediffDefOf.Scaria) || !pawn.HostileTo(f.GetActor())) ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnSomeonePhysicallyInteracting<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = delegate
			{
				Pawn actor = f.GetActor();
				Thing thing = actor.jobs.curJob.GetTarget(ind).Thing;
				return (thing == null || !actor.Map.physicalInteractionReservationManager.IsReserved(thing) || actor.Map.physicalInteractionReservationManager.IsReservedBy(actor, thing)) ? JobCondition.Ongoing : JobCondition.Incompletable;
			};
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnForbidden<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = delegate
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
				IntVec3 cell = actor.jobs.curJob.GetTarget(ind).Cell;
				Thing thing = actor.jobs.curJob.GetTarget(ind).Thing;
				IThingHolder thingHolder = thing?.ParentHolder;
				if (!(thingHolder is Pawn_CarryTracker) && !(thingHolder is Pawn_InventoryTracker) && cell.IsValid && cell.IsForbidden(actor))
				{
					return JobCondition.Incompletable;
				}
				if (thing == null)
				{
					return JobCondition.Ongoing;
				}
				return (!thing.IsForbidden(actor)) ? JobCondition.Ongoing : JobCondition.Incompletable;
			};
			reference.AddEndCondition(newEndCondition);
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
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = delegate
			{
				Pawn actor = f.GetActor();
				Job curJob = actor.jobs.curJob;
				if (curJob.ignoreDesignations)
				{
					return JobCondition.Ongoing;
				}
				Thing thing = curJob.GetTarget(ind).Thing;
				return (thing != null && actor.Map.designationManager.DesignationOn(thing, desDef) != null) ? JobCondition.Ongoing : JobCondition.Incompletable;
			};
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnThingHavingDesignation<T>(this T f, TargetIndex ind, DesignationDef desDef) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = delegate
			{
				Pawn actor = f.GetActor();
				Job curJob = actor.jobs.curJob;
				if (curJob.ignoreDesignations)
				{
					return JobCondition.Ongoing;
				}
				Thing thing = curJob.GetTarget(ind).Thing;
				return (thing != null && actor.Map.designationManager.DesignationOn(thing, desDef) == null) ? JobCondition.Ongoing : JobCondition.Incompletable;
			};
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnCellMissingDesignation<T>(this T f, TargetIndex ind, DesignationDef desDef) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = delegate
			{
				Pawn actor = f.GetActor();
				Job curJob = actor.jobs.curJob;
				if (curJob.ignoreDesignations)
				{
					return JobCondition.Ongoing;
				}
				return (actor.Map.designationManager.DesignationAt(curJob.GetTarget(ind).Cell, desDef) != null) ? JobCondition.Ongoing : JobCondition.Incompletable;
			};
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnBurningImmobile<T>(this T f, TargetIndex ind) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = delegate
			{
				Pawn actor = f.GetActor();
				LocalTargetInfo target = actor.jobs.curJob.GetTarget(ind);
				return (!target.IsValid || !target.ToTargetInfo(actor.Map).IsBurning()) ? JobCondition.Ongoing : JobCondition.Incompletable;
			};
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnCannotReach<T>(this T f, TargetIndex ind, PathEndMode mode) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = delegate
			{
				Pawn actor = f.GetActor();
				Job curJob = actor.jobs.curJob;
				return (!actor.IsHashIntervalTick(300) || actor.CanReach(curJob.GetTarget(ind), mode, Danger.Deadly)) ? JobCondition.Ongoing : JobCondition.Incompletable;
			};
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnCannotTouch<T>(this T f, TargetIndex ind, PathEndMode peMode) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => f.GetActor().CanReachImmediate(f.GetActor().jobs.curJob.GetTarget(ind), peMode) ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnIncapable<T>(this T f, PawnCapacityDef pawnCapacity) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = () => f.GetActor().health.capacities.CapableOf(pawnCapacity) ? JobCondition.Ongoing : JobCondition.Incompletable;
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static T FailOnChildLearningConditions<T>(this T f) where T : IJobEndable
		{
			ref T reference = ref f;
			Func<JobCondition> newEndCondition = delegate
			{
				Pawn actor = f.GetActor();
				return (actor.DevelopmentalStage.Juvenile() && !PawnUtility.WillSoonHaveBasicNeed(actor, -0.05f)) ? JobCondition.Ongoing : JobCondition.Incompletable;
			};
			reference.AddEndCondition(newEndCondition);
			return f;
		}

		public static Toil FailOnDespawnedNullOrForbiddenPlacedThings(this Toil toil, TargetIndex containerIndex = TargetIndex.None)
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
					ThingOwner thingOwner = toil.actor.jobs.curJob.GetTarget(containerIndex).Thing?.TryGetInnerInteractableThingOwner();
					if (thingCountClass.thing == null || (!thingCountClass.thing.Spawned && (thingOwner == null || !thingOwner.Contains(thingCountClass.thing))) || thingCountClass.thing.MapHeld != toil.actor.Map || (!toil.actor.CurJob.ignoreForbidden && thingCountClass.thing.IsForbidden(toil.actor)))
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
