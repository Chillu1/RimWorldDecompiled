using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_WanderControlGroupTarget : JobGiver_Wander
	{
		public JobGiver_WanderControlGroupTarget()
		{
			wanderRadius = 7f;
			ticksBetweenWandersRange = new IntRange(125, 200);
		}

		private GlobalTargetInfo Target(Pawn pawn)
		{
			return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer).mechanitor.GetControlGroup(pawn).Target;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (Target(pawn).Map != pawn.Map)
			{
				return null;
			}
			return base.TryGiveJob(pawn);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return Target(pawn).Cell;
		}

		protected override void DecorateGotoJob(Job job)
		{
			job.expiryInterval = 120;
			job.expireRequiresEnemiesNearby = true;
		}
	}
}
