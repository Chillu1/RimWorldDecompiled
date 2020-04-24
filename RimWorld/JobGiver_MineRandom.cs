using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_MineRandom : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Region region = pawn.GetRegion();
			if (region == null)
			{
				return null;
			}
			for (int i = 0; i < 40; i++)
			{
				IntVec3 randomCell = region.RandomCell;
				for (int j = 0; j < 4; j++)
				{
					IntVec3 c = randomCell + GenAdj.CardinalDirections[j];
					if (c.InBounds(pawn.Map))
					{
						Building edifice = c.GetEdifice(pawn.Map);
						if (edifice != null && (edifice.def.passability == Traversability.Impassable || edifice.def.IsDoor) && edifice.def.size == IntVec2.One && edifice.def != ThingDefOf.CollapsedRocks && pawn.CanReserve(edifice))
						{
							Job job = JobMaker.MakeJob(JobDefOf.Mine, edifice);
							job.ignoreDesignations = true;
							return job;
						}
					}
				}
			}
			return null;
		}
	}
}
