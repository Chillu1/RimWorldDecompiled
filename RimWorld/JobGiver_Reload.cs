using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Reload : ThinkNode_JobGiver
	{
		private const bool forceReloadWhenLookingForWork = false;

		public override float GetPriority(Pawn pawn)
		{
			return 5.9f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			CompReloadable compReloadable = ReloadableUtility.FindSomeReloadableComponent(pawn, allowForcedReload: false);
			if (compReloadable == null)
			{
				return null;
			}
			List<Thing> list = ReloadableUtility.FindEnoughAmmo(pawn, pawn.Position, compReloadable, forceReload: false);
			if (list == null)
			{
				return null;
			}
			return MakeReloadJob(compReloadable, list);
		}

		public static Job MakeReloadJob(CompReloadable comp, List<Thing> chosenAmmo)
		{
			Job job = JobMaker.MakeJob(JobDefOf.Reload, comp.parent);
			job.targetQueueB = chosenAmmo.Select((Thing t) => new LocalTargetInfo(t)).ToList();
			job.count = chosenAmmo.Sum((Thing t) => t.stackCount);
			job.count = Math.Min(job.count, comp.MaxAmmoNeeded(allowForcedReload: true));
			return job;
		}
	}
}
