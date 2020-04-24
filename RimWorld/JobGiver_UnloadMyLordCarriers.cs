using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_UnloadMyLordCarriers : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return null;
			}
			Lord lord = pawn.GetLord();
			if (lord == null)
			{
				return null;
			}
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				if (UnloadCarriersJobGiverUtility.HasJobOnThing(pawn, lord.ownedPawns[i], forced: false))
				{
					return JobMaker.MakeJob(JobDefOf.UnloadInventory, lord.ownedPawns[i]);
				}
			}
			return null;
		}
	}
}
