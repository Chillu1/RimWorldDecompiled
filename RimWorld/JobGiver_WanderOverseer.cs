using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_WanderOverseer : JobGiver_Wander
	{
		public JobGiver_WanderOverseer()
		{
			wanderRadius = 3f;
			ticksBetweenWandersRange = new IntRange(125, 200);
		}

		private GlobalTargetInfo Target(Pawn pawn)
		{
			return pawn.GetOverseer();
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			GlobalTargetInfo globalTargetInfo = Target(pawn);
			if (globalTargetInfo.Map != pawn.Map)
			{
				return null;
			}
			Job job = base.TryGiveJob(pawn);
			job.reportStringOverride = "Escorting".Translate(globalTargetInfo.Thing.Named("TARGET"));
			return job;
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
