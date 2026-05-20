using RimWorld;

namespace Verse.AI;

public class MentalStateWorker_Roaming : MentalStateWorker
{
	public const int GracePeriodSinceGameStartedDays = 5;

	public override bool StateCanOccur(Pawn pawn)
	{
		if (!base.StateCanOccur(pawn))
		{
			return false;
		}
		return CanRoamNow(pawn);
	}

	public static bool CanRoamNow(Pawn pawn)
	{
		if (pawn.Spawned && pawn.Map.IsPlayerHome && GenTicks.TicksGame >= 300000 && pawn.Roamer && pawn.Faction == Faction.OfPlayer && !pawn.roping.IsRoped && !pawn.mindState.InRoamingCooldown)
		{
			return pawn.CanReachMapEdge();
		}
		return false;
	}
}
