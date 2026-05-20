using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Steal : ThinkNode_JobGiver
{
	public const float ItemsSearchRadiusInitial = 7f;

	private const float ItemsSearchRadiusOngoing = 12f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!RCellFinder.TryFindBestExitSpot(pawn, out var spot))
		{
			return null;
		}
		if (StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 12f, out var item, pawn) && !GenAI.InDangerousCombat(pawn))
		{
			Job job = JobMaker.MakeJob(JobDefOf.Steal);
			job.targetA = item;
			job.targetB = spot;
			job.count = Mathf.Min(item.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity) / item.def.VolumePerUnit));
			return job;
		}
		return null;
	}
}
