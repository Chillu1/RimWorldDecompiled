using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Haul : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling(), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, Validator);
		if (thing != null)
		{
			return HaulAIUtility.HaulToStorageJob(pawn, thing, forced: false);
		}
		return null;
		bool Validator(Thing t)
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
			if (!StoreUtility.TryFindBestBetterStoreCellFor(t, pawn, pawn.Map, StoreUtility.CurrentStoragePriorityOf(t), pawn.Faction, out var _))
			{
				return false;
			}
			return true;
		}
	}
}
