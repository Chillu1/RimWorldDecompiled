using System;
using RimWorld;

namespace Verse.AI;

public static class Toils_General
{
	public static Toil StopDead()
	{
		Toil toil = ToilMaker.MakeToil("StopDead");
		toil.initAction = delegate
		{
			toil.actor.pather.StopDead();
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

	public static Toil Wait(int ticks, TargetIndex face = TargetIndex.None)
	{
		Toil toil = ToilMaker.MakeToil("Wait");
		toil.initAction = delegate
		{
			toil.actor.pather.StopDead();
		};
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = ticks;
		if (face != TargetIndex.None)
		{
			toil.handlingFacing = true;
			toil.tickIntervalAction = delegate
			{
				toil.actor.rotationTracker.FaceTarget(toil.actor.CurJob.GetTarget(face));
			};
		}
		return toil;
	}

	public static Toil WaitWith(TargetIndex targetInd, int ticks, bool useProgressBar = false, bool maintainPosture = false, bool maintainSleep = false, TargetIndex face = TargetIndex.None, PathEndMode pathEndMode = PathEndMode.Touch)
	{
		Toil toil = ToilMaker.MakeToil("WaitWith");
		toil.initAction = delegate
		{
			toil.actor.pather.StopDead();
			if (toil.actor.CurJob.GetTarget(targetInd).Thing is Pawn pawn)
			{
				if (pawn == toil.actor)
				{
					Log.Warning("Executing WaitWith toil but otherPawn is the same as toil.actor");
				}
				else
				{
					PawnUtility.ForceWait(pawn, ticks, null, maintainPosture, maintainSleep);
				}
			}
		};
		toil.FailOnDespawnedOrNull(targetInd);
		toil.FailOnCannotTouch(targetInd, pathEndMode);
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = ticks;
		if (face != TargetIndex.None)
		{
			toil.handlingFacing = true;
			toil.tickIntervalAction = delegate
			{
				toil.actor.rotationTracker.FaceTarget(toil.actor.CurJob.GetTarget(face));
			};
		}
		if (useProgressBar)
		{
			toil.WithProgressBarToilDelay(targetInd);
		}
		return toil;
	}

	public static Toil RemoveDesignationsOnThing(TargetIndex ind, DesignationDef def)
	{
		Toil toil = ToilMaker.MakeToil("RemoveDesignationsOnThing");
		toil.initAction = delegate
		{
			toil.actor.Map.designationManager.RemoveAllDesignationsOn(toil.actor.jobs.curJob.GetTarget(ind).Thing);
		};
		return toil;
	}

	public static Toil ClearTarget(TargetIndex ind)
	{
		Toil toil = ToilMaker.MakeToil("ClearTarget");
		toil.initAction = delegate
		{
			toil.GetActor().CurJob.SetTarget(ind, null);
		};
		return toil;
	}

	public static Toil PutCarriedThingInInventory()
	{
		Toil toil = ToilMaker.MakeToil("PutCarriedThingInInventory");
		toil.initAction = delegate
		{
			Pawn actor = toil.GetActor();
			if (actor.carryTracker.CarriedThing != null && !actor.carryTracker.innerContainer.TryTransferToContainer(actor.carryTracker.CarriedThing, actor.inventory.innerContainer))
			{
				actor.carryTracker.TryDropCarriedThing(actor.Position, actor.carryTracker.CarriedThing.stackCount, ThingPlaceMode.Near, out var _);
			}
		};
		return toil;
	}

	public static Toil Do(Action action)
	{
		Toil toil = ToilMaker.MakeToil("Do");
		toil.initAction = action;
		return toil;
	}

	public static Toil DoAtomic(Action action)
	{
		Toil toil = ToilMaker.MakeToil("DoAtomic");
		toil.initAction = action;
		toil.atomicWithPrevious = true;
		return toil;
	}

	public static Toil Open(TargetIndex openableInd)
	{
		Toil open = ToilMaker.MakeToil("Open");
		open.initAction = delegate
		{
			Pawn actor = open.actor;
			Thing thing = actor.CurJob.GetTarget(openableInd).Thing;
			actor.Map.designationManager.DesignationOn(thing, DesignationDefOf.Open)?.Delete();
			IOpenable openable = (IOpenable)thing;
			if (openable.CanOpen)
			{
				openable.Open();
				actor.records.Increment(RecordDefOf.ContainersOpened);
			}
		};
		open.defaultCompleteMode = ToilCompleteMode.Instant;
		return open;
	}

	public static Toil Label()
	{
		Toil toil = ToilMaker.MakeToil("Label");
		toil.atomicWithPrevious = true;
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

	public static Toil WaitWhileExtractingContents(TargetIndex containerInd, TargetIndex contentsInd, int openTicks)
	{
		Toil extract = Wait(openTicks, containerInd).WithProgressBarToilDelay(containerInd).FailOnDespawnedOrNull(containerInd);
		extract.handlingFacing = true;
		extract.AddPreInitAction(delegate
		{
			Thing thing = extract.actor.CurJob.GetTarget(contentsInd).Thing;
			QuestUtility.SendQuestTargetSignals(thing.questTags, "StartedExtractingFromContainer", thing.Named("SUBJECT"));
		});
		return extract;
	}
}
