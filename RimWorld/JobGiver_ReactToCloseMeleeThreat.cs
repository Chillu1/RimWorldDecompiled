using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_ReactToCloseMeleeThreat : ThinkNode_JobGiver
	{
		private const int MaxMeleeChaseTicks = 200;

		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn meleeThreat = pawn.mindState.meleeThreat;
			if (meleeThreat == null)
			{
				return null;
			}
			if (meleeThreat.IsInvisible())
			{
				return null;
			}
			if (IsHunting(pawn, meleeThreat))
			{
				return null;
			}
			if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
			{
				return null;
			}
			if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse && pawn.playerSettings.hostilityResponse != HostilityResponseMode.Attack)
			{
				return null;
			}
			if (!pawn.mindState.MeleeThreatStillThreat)
			{
				pawn.mindState.meleeThreat = null;
				return null;
			}
			if (pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, meleeThreat);
			job.maxNumMeleeAttacks = 1;
			job.expiryInterval = 200;
			job.reactingToMeleeThreat = true;
			return job;
		}

		private bool IsHunting(Pawn pawn, Pawn prey)
		{
			if (pawn.CurJob == null)
			{
				return false;
			}
			JobDriver_Hunt jobDriver_Hunt = pawn.jobs.curDriver as JobDriver_Hunt;
			if (jobDriver_Hunt != null)
			{
				return jobDriver_Hunt.Victim == prey;
			}
			JobDriver_PredatorHunt jobDriver_PredatorHunt = pawn.jobs.curDriver as JobDriver_PredatorHunt;
			if (jobDriver_PredatorHunt != null)
			{
				return jobDriver_PredatorHunt.Prey == prey;
			}
			return false;
		}
	}
}
