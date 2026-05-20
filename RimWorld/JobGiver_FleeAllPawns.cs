using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_FleeAllPawns : ThinkNode_JobGiver
{
	private static readonly List<Thing> tmpPawns = new List<Thing>();

	protected override Job TryGiveJob(Pawn pawn)
	{
		Region region = pawn.GetRegion();
		if (region == null)
		{
			return null;
		}
		RegionTraverser.BreadthFirstTraverse(region, (Region from, Region reg) => reg.door == null || reg.door.Open, delegate(Region reg)
		{
			List<Thing> list = reg.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != pawn && list[i] is IAttackTarget attackTarget && !attackTarget.ThreatDisabled(null) && list[i] is Pawn pawn2 && (pawn2.HostileTo(pawn) || pawn2.RaceProps.Humanlike) && GenSight.LineOfSightToThing(pawn.Position, pawn2, pawn.Map, skipFirstCell: true))
				{
					tmpPawns.Add(pawn2);
				}
			}
			return false;
		}, 9);
		if (tmpPawns.Any())
		{
			IntVec3 fleeDest = CellFinderLoose.GetFleeDest(pawn, tmpPawns);
			tmpPawns.Clear();
			if (fleeDest.IsValid && fleeDest != pawn.Position)
			{
				Job job = JobMaker.MakeJob(JobDefOf.FleeAndCowerShort, fleeDest);
				job.checkOverrideOnExpire = true;
				job.expiryInterval = 65;
				return job;
			}
		}
		return null;
	}
}
