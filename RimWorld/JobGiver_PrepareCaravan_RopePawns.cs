using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class JobGiver_PrepareCaravan_RopePawns : ThinkNode_JobGiver
	{
		protected abstract JobDef RopeJobDef { get; }

		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			Pawn pawn2 = ((!pawn.roping.IsRopingOthers) ? FindAnimalNeedingGathering(pawn) : pawn.roping.Ropees[0]);
			if (pawn2 == null)
			{
				return null;
			}
			Job job = JobMaker.MakeJob(RopeJobDef, pawn2, cell);
			job.lord = pawn.GetLord();
			DecorateJob(job);
			return job;
		}

		protected virtual void DecorateJob(Job job)
		{
		}

		private Pawn FindAnimalNeedingGathering(Pawn pawn)
		{
			foreach (Pawn ownedPawn in pawn.GetLord().ownedPawns)
			{
				if (AnimalNeedsGathering(pawn, ownedPawn))
				{
					return ownedPawn;
				}
			}
			return null;
		}

		protected abstract bool AnimalNeedsGathering(Pawn pawn, Pawn animal);
	}
}
