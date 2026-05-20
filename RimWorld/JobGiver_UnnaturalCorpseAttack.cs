using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_UnnaturalCorpseAttack : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.Downed)
		{
			return null;
		}
		if (!Find.Anomaly.TryGetUnnaturalCorpseTrackerForAwoken(pawn, out var tracker))
		{
			return null;
		}
		Pawn haunted = tracker.Haunted;
		if (haunted.DestroyedOrNull())
		{
			return null;
		}
		if (!haunted.Spawned)
		{
			if (haunted.SpawnedOrAnyParentSpawned && pawn.CanReach(haunted.SpawnedParentOrMe, PathEndMode.Touch, Danger.Deadly))
			{
				Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, haunted.SpawnedParentOrMe);
				job.expiryInterval = 30;
				return job;
			}
			return null;
		}
		if (pawn.CanReach(haunted, PathEndMode.Touch, Danger.Deadly))
		{
			Job job2 = JobMaker.MakeJob(JobDefOf.UnnaturalCorpseAttack, haunted);
			job2.expiryInterval = 120;
			job2.checkOverrideOnExpire = true;
			return job2;
		}
		return null;
	}
}
