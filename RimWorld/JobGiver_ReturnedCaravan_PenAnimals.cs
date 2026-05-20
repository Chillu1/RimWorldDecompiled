using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_ReturnedCaravan_PenAnimals : JobGiver_PrepareCaravan_RopePawns
	{
		protected override JobDef RopeJobDef => JobDefOf.ReturnedCaravan_PenAnimals;

		protected override bool AnimalNeedsGathering(Pawn pawn, Pawn animal)
		{
			return false;
		}

		protected override void DecorateJob(Job job)
		{
			base.DecorateJob(job);
			job.ropeToUnenclosedPens = true;
		}
	}
}
