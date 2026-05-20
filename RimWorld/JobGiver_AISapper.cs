using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_AISapper : ThinkNode_JobGiver
{
	private bool canMineMineables = true;

	private bool canMineNonMineables = true;

	private const float ReachDestDist = 10f;

	private const int CheckOverrideInterval = 500;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_AISapper obj = (JobGiver_AISapper)base.DeepCopy(resolve);
		obj.canMineMineables = canMineMineables;
		obj.canMineNonMineables = canMineNonMineables;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		IntVec3 intVec = pawn.mindState.duty.focus.Cell;
		if (intVec.IsValid && (float)intVec.DistanceToSquared(pawn.Position) < 100f && intVec.GetRoom(pawn.Map) == pawn.GetRoom() && intVec.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors))
		{
			pawn.GetLord().Notify_ReachedDutyLocation(pawn);
			return null;
		}
		if (!intVec.IsValid)
		{
			if (!(from x in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
				where !x.ThreatDisabled(pawn) && x.Thing.Faction == Faction.OfPlayer && pawn.Position.DistanceToSquared(x.Thing.Position) <= 2500 && pawn.CanReach(x.Thing, PathEndMode.OnCell, Danger.Deadly, canBashDoors: false, canBashFences: false, TraverseMode.PassAllDestroyableThings)
				select x).TryRandomElement(out var result))
			{
				return null;
			}
			intVec = result.Thing.Position;
		}
		if (!pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, canBashDoors: false, canBashFences: false, TraverseMode.PassAllDestroyableThings))
		{
			return null;
		}
		using (PawnPath path = pawn.Map.pathFinder.FindPathNow(pawn.Position, intVec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings)))
		{
			IntVec3 cellBefore;
			Thing thing = path.FirstBlockingBuilding(out cellBefore, pawn);
			if (thing != null)
			{
				Job job = DigUtility.PassBlockerJob(pawn, thing, cellBefore, canMineMineables, canMineNonMineables);
				if (job != null)
				{
					return job;
				}
			}
		}
		return JobMaker.MakeJob(JobDefOf.Goto, intVec, 500, checkOverrideOnExpiry: true);
	}
}
