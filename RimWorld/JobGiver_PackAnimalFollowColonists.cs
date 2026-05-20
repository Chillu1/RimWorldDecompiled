using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_PackAnimalFollowColonists : ThinkNode_JobGiver
{
	private const int MaxDistanceToPawnToFollow = 10;

	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn pawnToFollow = GetPawnToFollow(pawn);
		if (pawnToFollow == null)
		{
			return null;
		}
		if (pawnToFollow.Position.InHorDistOf(pawn.Position, 10f) && pawnToFollow.Position.WithinRegions(pawn.Position, pawn.Map, 5, TraverseParms.For(pawn)))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Follow, pawnToFollow);
		job.locomotionUrgency = LocomotionUrgency.Walk;
		job.checkOverrideOnExpire = true;
		job.expiryInterval = 120;
		return job;
	}

	public static Pawn GetPawnToFollow(Pawn forPawn)
	{
		if (!forPawn.RaceProps.packAnimal || forPawn.inventory.UnloadEverything || AnimalPenUtility.NeedsToBeManagedByRope(forPawn) || MassUtility.IsOverEncumbered(forPawn))
		{
			return null;
		}
		Lord lord = forPawn.GetLord();
		if (lord == null)
		{
			return null;
		}
		List<Pawn> ownedPawns = lord.ownedPawns;
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			Pawn pawn = ownedPawns[i];
			if (pawn != forPawn && CaravanUtility.IsOwner(pawn, forPawn.Faction) && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.PrepareCaravan_GatherItems && forPawn.CanReach(pawn, PathEndMode.Touch, Danger.Deadly) && ((JobDriver_PrepareCaravan_GatherItems)pawn.jobs.curDriver).Carrier == forPawn)
			{
				return pawn;
			}
		}
		Pawn pawn2 = null;
		for (int j = 0; j < ownedPawns.Count; j++)
		{
			Pawn pawn3 = ownedPawns[j];
			if (pawn3 != forPawn && CaravanUtility.IsOwner(pawn3, forPawn.Faction) && pawn3.CurJob != null && pawn3.CurJob.def == JobDefOf.PrepareCaravan_GatherItems && forPawn.CanReach(pawn3, PathEndMode.Touch, Danger.Deadly) && (pawn2 == null || forPawn.Position.DistanceToSquared(pawn3.Position) < forPawn.Position.DistanceToSquared(pawn2.Position)))
			{
				pawn2 = pawn3;
			}
		}
		return pawn2;
	}
}
