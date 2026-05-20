using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public static class MentalStateUtility
{
	public static MentalStateDef GetWanderToOwnRoomStateOrFallback(Pawn pawn)
	{
		if (MentalStateDefOf.Wander_OwnRoom.Worker.StateCanOccur(pawn))
		{
			return MentalStateDefOf.Wander_OwnRoom;
		}
		if (MentalStateDefOf.Wander_Sad.Worker.StateCanOccur(pawn))
		{
			return MentalStateDefOf.Wander_Sad;
		}
		return null;
	}

	public static void TryTransitionToWanderOwnRoom(MentalState mentalState)
	{
		MentalStateDef wanderToOwnRoomStateOrFallback = GetWanderToOwnRoomStateOrFallback(mentalState.pawn);
		if (wanderToOwnRoomStateOrFallback != null)
		{
			mentalState.pawn.mindState.mentalStateHandler.TryStartMentalState(wanderToOwnRoomStateOrFallback, null, forced: false, forceWake: false, mentalState.causedByMood, null, transitionSilently: true);
		}
		else
		{
			mentalState.RecoverFromState();
		}
	}

	public static void StartMentalState(Pawn pawn, MentalStateDef stateDef, MentalStateDef mechStateDef = null)
	{
		MentalStateDef stateDef2 = (pawn.RaceProps.IsMechanoid ? (mechStateDef ?? stateDef) : stateDef);
		if (pawn.mindState.mentalStateHandler.TryStartMentalState(stateDef2, null, forced: false, forceWake: true))
		{
			RestUtility.WakeUp(pawn);
		}
	}

	public static bool IsHavingMentalBreak(Pawn pawn)
	{
		if (!pawn.mindState.mentalStateHandler.InMentalState)
		{
			return false;
		}
		MentalStateDef mentalStateDef = pawn.MentalStateDef;
		if (mentalStateDef == null)
		{
			return false;
		}
		List<MentalBreakDef> allDefsListForReading = DefDatabase<MentalBreakDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			if (allDefsListForReading[i].mentalState == mentalStateDef)
			{
				return true;
			}
		}
		return false;
	}
}
