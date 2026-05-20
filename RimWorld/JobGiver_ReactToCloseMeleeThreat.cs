using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_ReactToCloseMeleeThreat : ThinkNode_JobGiver
{
	private float maxDistance = -1f;

	private const int MaxMeleeChaseTicks = 200;

	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn meleeThreat = pawn.mindState.meleeThreat;
		if (meleeThreat == null)
		{
			return null;
		}
		if (maxDistance > 0f && pawn.Position.DistanceTo(meleeThreat.Position) > maxDistance)
		{
			return null;
		}
		if (meleeThreat.IsPsychologicallyInvisible())
		{
			return null;
		}
		CompActivity comp = meleeThreat.GetComp<CompActivity>();
		if (comp != null && comp.IsDormant)
		{
			return null;
		}
		if (IsHunting(pawn, meleeThreat))
		{
			return null;
		}
		if (IsDueling(pawn, meleeThreat))
		{
			return null;
		}
		if (IsHateChanting(pawn))
		{
			return null;
		}
		if (pawn.IsAwokenCorpse)
		{
			return null;
		}
		if (MentalStateUtility.IsHavingMentalBreak(pawn))
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

	private bool IsDueling(Pawn pawn, Pawn other)
	{
		if (pawn.GetLord()?.LordJob is LordJob_Ritual_Duel lordJob_Ritual_Duel)
		{
			return lordJob_Ritual_Duel.Opponent(pawn) == other;
		}
		return false;
	}

	private bool IsHunting(Pawn pawn, Pawn prey)
	{
		if (pawn.CurJob == null)
		{
			return false;
		}
		if (pawn.jobs.curDriver is JobDriver_Hunt jobDriver_Hunt)
		{
			return jobDriver_Hunt.Victim == prey;
		}
		if (pawn.jobs.curDriver is JobDriver_PredatorHunt jobDriver_PredatorHunt)
		{
			return jobDriver_PredatorHunt.Prey == prey;
		}
		return false;
	}

	private bool IsHateChanting(Pawn pawn)
	{
		if (ModsConfig.AnomalyActive)
		{
			return pawn.GetLord()?.LordJob is LordJob_HateChant;
		}
		return false;
	}
}
