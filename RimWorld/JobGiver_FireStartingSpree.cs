using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	internal class JobGiver_FireStartingSpree : ThinkNode_JobGiver
	{
		private IntRange waitTicks = new IntRange(80, 140);

		private const float FireStartChance = 0.75f;

		private static List<Thing> potentialTargets = new List<Thing>();

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_FireStartingSpree obj = (JobGiver_FireStartingSpree)base.DeepCopy(resolve);
			obj.waitTicks = waitTicks;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.mindState.nextMoveOrderIsWait)
			{
				Job job = JobMaker.MakeJob(JobDefOf.Wait_Wander);
				job.expiryInterval = waitTicks.RandomInRange;
				pawn.mindState.nextMoveOrderIsWait = false;
				return job;
			}
			if (Rand.Value < 0.75f)
			{
				Thing thing = TryFindRandomIgniteTarget(pawn);
				if (thing != null)
				{
					pawn.mindState.nextMoveOrderIsWait = true;
					return JobMaker.MakeJob(JobDefOf.Ignite, thing);
				}
			}
			IntVec3 c = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 10f, null, Danger.Deadly);
			if (c.IsValid)
			{
				pawn.mindState.nextMoveOrderIsWait = true;
				return JobMaker.MakeJob(JobDefOf.GotoWander, c);
			}
			return null;
		}

		private Thing TryFindRandomIgniteTarget(Pawn pawn)
		{
			if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(), TraverseParms.For(pawn), (Region candidateRegion) => !candidateRegion.IsForbiddenEntirely(pawn), 100, out var result))
			{
				return null;
			}
			potentialTargets.Clear();
			List<Thing> allThings = result.ListerThings.AllThings;
			for (int i = 0; i < allThings.Count; i++)
			{
				Thing thing = allThings[i];
				if ((thing.def.category == ThingCategory.Building || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Plant) && thing.FlammableNow && !thing.IsBurning() && !thing.OccupiedRect().Contains(pawn.Position))
				{
					potentialTargets.Add(thing);
				}
			}
			if (potentialTargets.NullOrEmpty())
			{
				return null;
			}
			return potentialTargets.RandomElement();
		}
	}
}
