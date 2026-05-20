using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_RevenantEscape : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		IntVec3 intVec = RevenantUtility.FindEscapeCell(pawn);
		if (!intVec.IsValid)
		{
			return null;
		}
		using (PawnPath pawnPath = pawn.Map.pathFinder.FindPathNow(pawn.Position, intVec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors)))
		{
			if (!pawnPath.Found)
			{
				using PawnPath path = pawn.Map.pathFinder.FindPathNow(pawn.Position, intVec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings));
				IntVec3 cellBefore;
				Thing thing = path.FirstBlockingBuilding(out cellBefore, pawn);
				if (thing != null)
				{
					Job job = DigUtility.PassBlockerJob(pawn, thing, cellBefore, canMineMineables: true, canMineNonMineables: true);
					if (job != null)
					{
						return job;
					}
				}
			}
		}
		Job job2 = JobMaker.MakeJob(JobDefOf.RevenantEscape, intVec);
		job2.locomotionUrgency = LocomotionUrgency.Sprint;
		job2.canBashDoors = true;
		job2.canBashFences = true;
		return job2;
	}
}
