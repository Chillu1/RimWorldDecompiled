using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_LayDownResting : JobDriver_LayDownAwake
	{
		public override Rot4 ForcedLayingRotation => Rot4.Invalid;

		public override Toil LayDownToil(bool hasBed)
		{
			return Toils_LayDown.LayDown(TargetIndex.A, hasBed, LookForOtherJobs, CanSleep, CanRest, PawnPosture.LayingOnGroundFaceUp);
		}
	}
}
