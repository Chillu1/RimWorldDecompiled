using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_FleeDanger : ThinkNode_JobGiver
{
	private const int FleeDistance = 10;

	private const int DistToFireToFlee = 4;

	private const int DistToDangerToFlee = 4;

	public override string CrawlingReportStringOverride => base.CrawlingReportStringOverride ?? ((string)"ReportStringCrawlingToSafety".Translate());

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.Downed && !pawn.health.CanCrawl)
		{
			return null;
		}
		Job job = FleeUtility.FleeLargeFireJob(pawn, 1, 4, 10);
		if (job != null)
		{
			return job;
		}
		List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
		for (int i = 0; i < potentialTargetsFor.Count; i++)
		{
			Thing thing = potentialTargetsFor[i].Thing;
			if (pawn.Position.InHorDistOf(thing.Position, 4f) && FleeUtility.ShouldFleeFrom(thing, pawn, checkDistance: false, checkLOS: true) && (!(thing is Pawn pawn2) || !pawn2.AnimalOrWildMan() || pawn2.Faction != null))
			{
				Job job2 = FleeUtility.FleeJob(pawn, thing, 10);
				if (job2 != null)
				{
					return job2;
				}
			}
		}
		return null;
	}
}
