using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobDriver_PlayStatic : JobDriver_BabyPlay
{
	private const int InteractionIntervalTicks = 1250;

	protected override StartingConditions StartingCondition => StartingConditions.GotoBaby;

	protected override IEnumerable<Toil> Play()
	{
		Toil toil = ToilMaker.MakeToil("Play");
		toil.WithEffect(EffecterDefOf.PlayStatic, TargetIndex.A);
		toil.handlingFacing = true;
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceTarget(base.Baby);
			if (pawn.IsHashIntervalTick(1250, delta))
			{
				pawn.interactions.TryInteractWith(base.Baby, InteractionDefOf.BabyPlay);
			}
			if (roomPlayGainFactor < 0f)
			{
				roomPlayGainFactor = BabyPlayUtility.GetRoomPlayGainFactors(base.Baby);
			}
			if (BabyPlayUtility.PlayTickCheckEnd(base.Baby, pawn, roomPlayGainFactor, delta))
			{
				pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		ChildcareUtility.MakeBabyPlayAsLongAsToilIsActive(toil, TargetIndex.A);
		yield return toil;
	}
}
