using System;
using RimWorld;

namespace Verse.AI;

public static class ToilJumpConditions
{
	public static Toil JumpIf(this Toil toil, Func<bool> jumpCondition, Toil jumpToil)
	{
		toil.AddPreTickAction(delegate
		{
			if (jumpCondition())
			{
				toil.actor.jobs.curDriver.JumpToToil(jumpToil);
			}
		});
		return toil;
	}

	public static Toil JumpIfDespawnedOrNull(this Toil toil, TargetIndex ind, Toil jumpToil)
	{
		return toil.JumpIf(delegate
		{
			Thing thing = toil.actor.jobs.curJob.GetTarget(ind).Thing;
			return thing == null || !thing.Spawned;
		}, jumpToil);
	}

	public static Toil JumpIfDespawnedOrNullOrForbidden(this Toil toil, TargetIndex ind, Toil jumpToil)
	{
		return toil.JumpIf(delegate
		{
			Thing thing = toil.actor.jobs.curJob.GetTarget(ind).Thing;
			return thing == null || !thing.Spawned || thing.IsForbidden(toil.actor);
		}, jumpToil);
	}

	public static Toil JumpIfOutsideHomeArea(this Toil toil, TargetIndex ind, Toil jumpToil)
	{
		return toil.JumpIf(delegate
		{
			Thing thing = toil.actor.jobs.curJob.GetTarget(ind).Thing;
			return !toil.actor.Map.areaManager.Home[thing.Position];
		}, jumpToil);
	}

	public static Toil JumpIfThingMissingDesignation(this Toil toil, TargetIndex ind, DesignationDef desDef, Toil jumpToil)
	{
		return toil.JumpIf(delegate
		{
			Pawn actor = toil.GetActor();
			Job curJob = actor.jobs.curJob;
			if (curJob.ignoreDesignations)
			{
				return false;
			}
			Thing thing = curJob.GetTarget(ind).Thing;
			return (thing == null || actor.Map.designationManager.DesignationOn(thing, desDef) == null) ? true : false;
		}, jumpToil);
	}
}
