using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_PrisonerEscape : ThinkNode_JobGiver
{
	private const int MaxRegionsToCheckWhenEscapingThroughOpenDoors = 25;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (ShouldStartEscaping(pawn) && RCellFinder.TryFindBestExitSpot(pawn, out var spot))
		{
			if (!pawn.guest.Released)
			{
				Messages.Message("MessagePrisonerIsEscaping".Translate(pawn.LabelShort, pawn), pawn, MessageTypeDefOf.ThreatSmall);
				Find.TickManager.slower.SignalForceNormalSpeed();
			}
			Job job = JobMaker.MakeJob(JobDefOf.Goto, spot);
			job.exitMapOnArrival = true;
			return job;
		}
		return null;
	}

	private bool ShouldStartEscaping(Pawn pawn)
	{
		if (!pawn.guest.IsPrisoner || pawn.guest.HostFaction != Faction.OfPlayer || !pawn.guest.PrisonerIsSecure)
		{
			return false;
		}
		Lord lord = pawn.GetLord();
		if (lord != null && lord.PrisonerSecure(pawn))
		{
			return false;
		}
		District district = pawn.GetDistrict();
		if (district.TouchesMapEdge)
		{
			return true;
		}
		bool found = false;
		RegionTraverser.BreadthFirstTraverse(district.Regions[0], (Region from, Region reg) => (reg.door == null || reg.door.FreePassage) ? true : false, delegate(Region reg)
		{
			if (reg.District.TouchesMapEdge)
			{
				found = true;
				return true;
			}
			return false;
		}, 25);
		if (found)
		{
			return true;
		}
		return false;
	}
}
