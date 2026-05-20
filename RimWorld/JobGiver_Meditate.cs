using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Meditate : ThinkNode_JobGiver
{
	public override float GetPriority(Pawn pawn)
	{
		Pawn_PsychicEntropyTracker psychicEntropy = pawn.psychicEntropy;
		bool flag = pawn.HasPsylink && psychicEntropy != null && psychicEntropy.CurrentPsyfocus < Mathf.Min(psychicEntropy.TargetPsyfocus, 0.95f) && psychicEntropy.PsychicSensitivity > float.Epsilon;
		if (!ValidatePawnState(pawn))
		{
			return 0f;
		}
		if (pawn.timetable?.CurrentAssignment == TimeAssignmentDefOf.Meditate)
		{
			return 9f;
		}
		if (pawn.CurrentBed() == null)
		{
			if (pawn.timetable?.CurrentAssignment == TimeAssignmentDefOf.Anything && flag)
			{
				return 7.1f;
			}
		}
		else if (flag && pawn.health.hediffSet.PainTotal <= 0.3f && pawn.CurrentBed() != null)
		{
			return 6f;
		}
		return 0f;
	}

	protected virtual bool ValidatePawnState(Pawn pawn)
	{
		if (pawn.CurrentBed() == null)
		{
			return !MeditationUtility.CanOnlyMeditateInBed(pawn);
		}
		return false;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!ModsConfig.RoyaltyActive)
		{
			return null;
		}
		if (!MeditationUtility.CanMeditateNow(pawn))
		{
			return null;
		}
		return MeditationUtility.GetMeditationJob(pawn);
	}
}
