using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetAgeReversal : ThinkNode_JobGiver
	{
		public CompBiosculpterPod GetBiosculpterPod(Pawn pawn)
		{
			List<CompBiosculpterPod> list = CompBiosculpterPod.BiotunedPods(pawn);
			if (list != null && CompBiosculpterPod.CanAgeReverse(pawn) && pawn.ageTracker.AgeReversalDemandedDeadlineTicks <= 0)
			{
				foreach (CompBiosculpterPod item in list)
				{
					if (item.parent.Spawned && item.AutoAgeReversal && item.CanAcceptOnceCycleChosen(pawn) && item.PawnCanUseNow(pawn, item.GetCycle(item.AgeReversalCycleKey)))
					{
						return item;
					}
				}
			}
			return null;
		}

		public override float GetPriority(Pawn pawn)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return 0f;
			}
			if (GetBiosculpterPod(pawn) != null)
			{
				return 7.4f;
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			CompBiosculpterPod biosculpterPod = GetBiosculpterPod(pawn);
			if (biosculpterPod == null || !pawn.CanReserve(biosculpterPod.parent))
			{
				return null;
			}
			Job job = biosculpterPod.EnterBiosculpterJob();
			biosculpterPod.ConfigureJobForCycle(job, biosculpterPod.GetCycle(biosculpterPod.AgeReversalCycleKey), null);
			return job;
		}
	}
}
