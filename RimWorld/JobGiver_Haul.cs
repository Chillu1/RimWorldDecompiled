using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Haul : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (t.IsForbidden(pawn))
				{
					return false;
				}
				if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced: false))
				{
					return false;
				}
				if (pawn.carryTracker.MaxStackSpaceEver(t.def) <= 0)
				{
					return false;
				}
				IntVec3 foundCell;
				return StoreUtility.TryFindBestBetterStoreCellFor(t, pawn, pawn.Map, StoreUtility.CurrentStoragePriorityOf(t), pawn.Faction, out foundCell) ? true : false;
			};
			Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling(), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, validator);
			if (thing != null)
			{
				return HaulAIUtility.HaulToStorageJob(pawn, thing);
			}
			return null;
		}
	}
}
