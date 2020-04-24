using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_DoLovin : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (Find.TickManager.TicksGame < pawn.mindState.canLovinTick)
			{
				return null;
			}
			if (pawn.CurrentBed() == null || pawn.CurrentBed().Medical || !pawn.health.capacities.CanBeAwake)
			{
				return null;
			}
			Pawn partnerInMyBed = LovePartnerRelationUtility.GetPartnerInMyBed(pawn);
			if (partnerInMyBed == null || !partnerInMyBed.health.capacities.CanBeAwake || Find.TickManager.TicksGame < partnerInMyBed.mindState.canLovinTick)
			{
				return null;
			}
			if (!pawn.CanReserve(partnerInMyBed) || !partnerInMyBed.CanReserve(pawn))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Lovin, partnerInMyBed, pawn.CurrentBed());
		}
	}
}
