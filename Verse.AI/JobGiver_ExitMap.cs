using RimWorld;

namespace Verse.AI
{
	public abstract class JobGiver_ExitMap : ThinkNode_JobGiver
	{
		protected LocomotionUrgency defaultLocomotion;

		protected int jobMaxDuration = 999999;

		protected bool canBash;

		protected bool forceCanDig;

		protected bool forceCanDigIfAnyHostileActiveThreat;

		protected bool forceCanDigIfCantReachMapEdge;

		protected bool failIfCantJoinOrCreateCaravan;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_ExitMap obj = (JobGiver_ExitMap)base.DeepCopy(resolve);
			obj.defaultLocomotion = defaultLocomotion;
			obj.jobMaxDuration = jobMaxDuration;
			obj.canBash = canBash;
			obj.forceCanDig = forceCanDig;
			obj.forceCanDigIfAnyHostileActiveThreat = forceCanDigIfAnyHostileActiveThreat;
			obj.forceCanDigIfCantReachMapEdge = forceCanDigIfCantReachMapEdge;
			obj.failIfCantJoinOrCreateCaravan = failIfCantJoinOrCreateCaravan;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			bool flag = forceCanDig || (pawn.mindState.duty != null && pawn.mindState.duty.canDig && !pawn.CanReachMapEdge()) || (forceCanDigIfCantReachMapEdge && !pawn.CanReachMapEdge()) || (forceCanDigIfAnyHostileActiveThreat && pawn.Faction != null && GenHostility.AnyHostileActiveThreatTo(pawn.Map, pawn.Faction, countDormantPawnsAsHostile: true));
			if (!TryFindGoodExitDest(pawn, flag, out IntVec3 dest))
			{
				return null;
			}
			if (flag)
			{
				using (PawnPath path = pawn.Map.pathFinder.FindPath(pawn.Position, dest, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings)))
				{
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
			Job job2 = JobMaker.MakeJob(JobDefOf.Goto, dest);
			job2.exitMapOnArrival = true;
			job2.failIfCantJoinOrCreateCaravan = failIfCantJoinOrCreateCaravan;
			job2.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, defaultLocomotion, LocomotionUrgency.Jog);
			job2.expiryInterval = jobMaxDuration;
			job2.canBash = canBash;
			return job2;
		}

		protected abstract bool TryFindGoodExitDest(Pawn pawn, bool canDig, out IntVec3 dest);
	}
}
