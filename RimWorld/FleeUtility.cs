using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class FleeUtility
{
	private static int MinFiresNearbyRadius = 20;

	private static int MinFiresNearbyRegionsToScan = 18;

	public const float FleeWhenDistToHostileLessThan = 8f;

	private static List<Thing> tmpThings = new List<Thing>();

	public static Job FleeJob(Pawn pawn, Thing danger, int fleeDistance)
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
			intVec = CellFinderLoose.GetFleeDest(pawn, tmpThings, fleeDistance);
			tmpThings.Clear();
		}
		if (intVec != pawn.Position)
		{
			return JobMaker.MakeJob(JobDefOf.Flee, intVec, danger);
		}
		return null;
	}

	public static Job FleeLargeFireJob(Pawn pawn, int minFiresNearbyToFlee, int distToFireToFlee, int fleeDistance)
	{
		if (pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Fire).Count < minFiresNearbyToFlee)
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
				if (!(num > (float)(MinFiresNearbyRadius * MinFiresNearbyRadius)))
				{
					if (closestFire == null || num < closestDistSq)
					{
						closestDistSq = num;
						closestFire = (Fire)list[i];
					}
					firesCount++;
				}
			}
			return closestDistSq <= (float)(distToFireToFlee * distToFireToFlee) && firesCount >= minFiresNearbyToFlee;
		}, MinFiresNearbyRegionsToScan);
		if (closestDistSq <= (float)(distToFireToFlee * distToFireToFlee) && firesCount >= minFiresNearbyToFlee)
		{
			Job job = FleeJob(pawn, closestFire, fleeDistance);
			if (job != null)
			{
				return job;
			}
		}
		return null;
	}

	public static bool ShouldAnimalFleeDanger(Pawn pawn)
	{
		if (pawn.IsAnimal && !pawn.InMentalState && !pawn.IsFighting() && !pawn.Downed && !pawn.Dead && !ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn) && pawn.GetLord() == null && (pawn.Faction != Faction.OfPlayer || !pawn.Map.IsPlayerHome) && (pawn.Faction == null || pawn.Faction.def.animalsFleeDanger) && (pawn.CurJob == null || !pawn.CurJobDef.neverFleeFromEnemies))
		{
			if (pawn.CurJob != null && pawn.jobs.curJob.def == JobDefOf.Flee)
			{
				return pawn.jobs.curJob.startTick != Find.TickManager.TicksGame;
			}
			return true;
		}
		return false;
	}

	public static bool ShouldFleeFrom(Thing t, Pawn pawn, bool checkDistance, bool checkLOS)
	{
		if (t == pawn || (checkDistance && !t.Position.InHorDistOf(pawn.Position, 8f)))
		{
			return false;
		}
		if (t.def.alwaysFlee)
		{
			return true;
		}
		if (!t.HostileTo(pawn))
		{
			return false;
		}
		if (!(t is IAttackTarget attackTarget) || attackTarget.ThreatDisabled(pawn) || !(t is IAttackTargetSearcher) || (checkLOS && !GenSight.LineOfSight(pawn.Position, t.Position, pawn.Map)))
		{
			return false;
		}
		return true;
	}
}
