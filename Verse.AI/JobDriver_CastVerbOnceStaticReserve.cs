namespace Verse.AI;

public class JobDriver_CastVerbOnceStaticReserve : JobDriver_CastVerbOnceStatic
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
	}
}
