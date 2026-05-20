namespace Verse.AI;

public class JobDriver_CastJump : JobDriver_CastVerbOnceStatic
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
		return true;
	}
}
