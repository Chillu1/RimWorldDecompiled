using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_HateChant : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil stand = Toils_General.Wait(int.MaxValue);
		stand.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceCell(pawn.Map.Center);
			pawn.GainComfortFromCellIfPossible(delta);
			Pawn actor = stand.actor;
			if (actor.IsHashIntervalTick(100, delta))
			{
				actor.jobs.CheckForJobOverride();
			}
		};
		stand.socialMode = RandomSocialMode.Off;
		stand.defaultCompleteMode = ToilCompleteMode.Never;
		stand.handlingFacing = true;
		yield return stand;
	}
}
