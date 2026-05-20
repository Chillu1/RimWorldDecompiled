using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Utility;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Reload : ThinkNode_JobGiver
{
	private const bool ForceReloadWhenLookingForWork = false;

	public override float GetPriority(Pawn pawn)
	{
		return 5.9f;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return null;
		}
		IReloadableComp reloadableComp = ReloadableUtility.FindSomeReloadableComponent(pawn, allowForcedReload: false);
		if (reloadableComp == null)
		{
			return null;
		}
		if (pawn.carryTracker.AvailableStackSpace(reloadableComp.AmmoDef) < reloadableComp.MinAmmoNeeded(allowForcedReload: true))
		{
			return null;
		}
		List<Thing> list = ReloadableUtility.FindEnoughAmmo(pawn, pawn.Position, reloadableComp, forceReload: false);
		if (list.NullOrEmpty())
		{
			return null;
		}
		return MakeReloadJob(reloadableComp, list);
	}

	public static Job MakeReloadJob(IReloadableComp reloadable, List<Thing> chosenAmmo)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Reload, reloadable.ReloadableThing);
		job.targetQueueB = chosenAmmo.Select((Thing t) => new LocalTargetInfo(t)).ToList();
		job.count = chosenAmmo.Sum((Thing t) => t.stackCount);
		job.count = Math.Min(job.count, reloadable.MaxAmmoNeeded(allowForcedReload: true));
		return job;
	}
}
