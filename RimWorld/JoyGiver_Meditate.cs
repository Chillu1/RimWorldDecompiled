using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_Meditate : JoyGiver_InPrivateRoom
{
	public override Job TryGiveJob(Pawn pawn)
	{
		if (ModsConfig.RoyaltyActive)
		{
			return MeditationUtility.GetMeditationJob(pawn, forJoy: true);
		}
		return base.TryGiveJob(pawn);
	}

	public override bool CanBeGivenTo(Pawn pawn)
	{
		if (ModsConfig.RoyaltyActive && !MeditationUtility.CanMeditateNow(pawn))
		{
			return false;
		}
		return base.CanBeGivenTo(pawn);
	}
}
