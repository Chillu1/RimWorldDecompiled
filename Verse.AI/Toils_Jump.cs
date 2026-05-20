using System;

namespace Verse.AI;

public static class Toils_Jump
{
	public static Toil Jump(Toil jumpTarget)
	{
		Toil toil = ToilMaker.MakeToil("Jump");
		toil.initAction = delegate
		{
			toil.actor.jobs.curDriver.JumpToToil(jumpTarget);
		};
		return toil;
	}

	public static Toil JumpIf(Toil jumpTarget, Func<bool> condition)
	{
		Toil toil = ToilMaker.MakeToil("JumpIf");
		toil.initAction = delegate
		{
			if (condition())
			{
				toil.actor.jobs.curDriver.JumpToToil(jumpTarget);
			}
		};
		return toil;
	}

	public static Toil JumpIfTargetDespawnedOrNull(TargetIndex ind, Toil jumpToil)
	{
		Toil toil = ToilMaker.MakeToil("JumpIfTargetDespawnedOrNull");
		toil.initAction = delegate
		{
			Thing thing = toil.actor.jobs.curJob.GetTarget(ind).Thing;
			if (thing == null || !thing.Spawned)
			{
				toil.actor.jobs.curDriver.JumpToToil(jumpToil);
			}
		};
		return toil;
	}

	public static Toil JumpIfTargetInvalid(TargetIndex ind, Toil jumpToil)
	{
		Toil toil = ToilMaker.MakeToil("JumpIfTargetInvalid");
		toil.initAction = delegate
		{
			if (!toil.actor.jobs.curJob.GetTarget(ind).IsValid)
			{
				toil.actor.jobs.curDriver.JumpToToil(jumpToil);
			}
		};
		return toil;
	}

	public static Toil JumpIfTargetNotHittable(TargetIndex ind, Toil jumpToil)
	{
		Toil toil = ToilMaker.MakeToil("JumpIfTargetNotHittable");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Job curJob = actor.jobs.curJob;
			LocalTargetInfo target = curJob.GetTarget(ind);
			if (curJob.verbToUse == null || !curJob.verbToUse.IsStillUsableBy(actor) || !curJob.verbToUse.CanHitTarget(target))
			{
				actor.jobs.curDriver.JumpToToil(jumpToil);
			}
		};
		return toil;
	}

	public static Toil JumpIfTargetDowned(TargetIndex ind, Toil jumpToil)
	{
		Toil toil = ToilMaker.MakeToil("JumpIfTargetDowned");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			if (actor.jobs.curJob.GetTarget(ind).Thing is Pawn { Downed: not false })
			{
				actor.jobs.curDriver.JumpToToil(jumpToil);
			}
		};
		return toil;
	}

	public static Toil JumpIfHaveTargetInQueue(TargetIndex ind, Toil jumpToil)
	{
		Toil toil = ToilMaker.MakeToil("JumpIfHaveTargetInQueue");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			if (!actor.jobs.curJob.GetTargetQueue(ind).NullOrEmpty())
			{
				actor.jobs.curDriver.JumpToToil(jumpToil);
			}
		};
		return toil;
	}

	public static Toil JumpIfCannotTouch(TargetIndex ind, PathEndMode peMode, Toil jumpToil)
	{
		Toil toil = ToilMaker.MakeToil("JumpIfCannotTouch");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			LocalTargetInfo target = actor.jobs.curJob.GetTarget(ind);
			if (!actor.CanReachImmediate(target, peMode))
			{
				actor.jobs.curDriver.JumpToToil(jumpToil);
			}
		};
		return toil;
	}
}
