using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_AnimalFlee : ThinkNode_JobGiver
	{
		private const int FleeDistance = 24;

		private const int DistToDangerToFlee = 18;

		private const int DistToFireToFlee = 10;

		private const int MinFiresNearbyToFlee = 60;

		private const int MinFiresNearbyRadius = 20;

		private const int MinFiresNearbyRegionsToScan = 18;

		private static List<Thing> tmpThings = new List<Thing>();

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse)
			{
				return null;
			}
			if (ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn))
			{
				return null;
			}
			if (pawn.Faction == null)
			{
				List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
				for (int i = 0; i < list.Count; i++)
				{
					if (pawn.Position.InHorDistOf(list[i].Position, 18f) && SelfDefenseUtility.ShouldFleeFrom(list[i], pawn, checkDistance: false, checkLOS: false))
					{
						Job job = FleeJob(pawn, list[i]);
						if (job != null)
						{
							return job;
						}
					}
				}
				Job job2 = FleeLargeFireJob(pawn);
				if (job2 != null)
				{
					return job2;
				}
			}
			else if (pawn.GetLord() == null && (pawn.Faction != Faction.OfPlayer || !pawn.Map.IsPlayerHome) && (pawn.CurJob == null || !pawn.CurJobDef.neverFleeFromEnemies))
			{
				List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
				for (int j = 0; j < potentialTargetsFor.Count; j++)
				{
					Thing thing = potentialTargetsFor[j].Thing;
					if (!pawn.Position.InHorDistOf(thing.Position, 18f) || !SelfDefenseUtility.ShouldFleeFrom(thing, pawn, checkDistance: false, checkLOS: true))
					{
						continue;
					}
					Pawn pawn2 = thing as Pawn;
					if (pawn2 == null || !pawn2.AnimalOrWildMan() || pawn2.Faction != null)
					{
						Job job3 = FleeJob(pawn, thing);
						if (job3 != null)
						{
							return job3;
						}
					}
				}
			}
			return null;
		}

		private Job FleeJob(Pawn pawn, Thing danger)
		{
			IntVec3 intVec;
			if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Flee)
			{
				intVec = pawn.CurJob.targetA.Cell;
			}
			else
			{
				tmpThings.Clear();
				tmpThings.Add(danger);
				intVec = CellFinderLoose.GetFleeDest(pawn, tmpThings, 24f);
				tmpThings.Clear();
			}
			if (intVec != pawn.Position)
			{
				return JobMaker.MakeJob(JobDefOf.Flee, intVec, danger);
			}
			return null;
		}

		private Job FleeLargeFireJob(Pawn pawn)
		{
			if (pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Fire).Count < 60)
			{
				return null;
			}
			TraverseParms tp = TraverseParms.For(pawn);
			Fire closestFire = null;
			float closestDistSq = -1f;
			int firesCount = 0;
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(tp, isDestination: false), delegate(Region x)
			{
				List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Fire);
				for (int i = 0; i < list.Count; i++)
				{
					float num = pawn.Position.DistanceToSquared(list[i].Position);
					if (!(num > 400f))
					{
						if (closestFire == null || num < closestDistSq)
						{
							closestDistSq = num;
							closestFire = (Fire)list[i];
						}
						firesCount++;
					}
				}
				return closestDistSq <= 100f && firesCount >= 60;
			}, 18);
			if (closestDistSq <= 100f && firesCount >= 60)
			{
				Job job = FleeJob(pawn, closestFire);
				if (job != null)
				{
					return job;
				}
			}
			return null;
		}
	}
}
