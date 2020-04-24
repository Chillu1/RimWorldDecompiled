using RimWorld;

namespace Verse.AI
{
	public class MentalState_Jailbreaker : MentalState
	{
		private const int NoPrisonerToFreeCheckInterval = 500;

		public override void MentalStateTick()
		{
			base.MentalStateTick();
			if (pawn.IsHashIntervalTick(500) && pawn.CurJobDef != JobDefOf.InducePrisonerToEscape && JailbreakerMentalStateUtility.FindPrisoner(pawn) == null)
			{
				RecoverFromState();
			}
		}

		public void Notify_InducedPrisonerToEscape()
		{
			if (MentalStateDefOf.Wander_OwnRoom.Worker.StateCanOccur(pawn))
			{
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_OwnRoom, null, forceWake: false, causedByMood, null, transitionSilently: true);
			}
			else if (MentalStateDefOf.Wander_Sad.Worker.StateCanOccur(pawn))
			{
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Sad, null, forceWake: false, causedByMood, null, transitionSilently: true);
			}
			else
			{
				RecoverFromState();
			}
		}
	}
}
