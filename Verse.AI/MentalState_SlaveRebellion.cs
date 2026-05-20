using RimWorld;

namespace Verse.AI;

public class MentalState_SlaveRebellion : MentalState
{
	private const int NoSlaveToRebelWithCheckInterval = 500;

	public override void MentalStateTick(int delta)
	{
		base.MentalStateTick(delta);
		if (pawn.IsHashIntervalTick(500, delta) && pawn.CurJobDef != JobDefOf.InduceSlaveToRebel && SlaveRebellionUtility.FindSlaveForRebellion(pawn) == null)
		{
			RecoverFromState();
		}
	}
}
