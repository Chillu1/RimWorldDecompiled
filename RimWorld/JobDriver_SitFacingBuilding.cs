using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_SitFacingBuilding : JobDriver
{
	private readonly TargetIndex BuildingIndex = TargetIndex.A;

	private readonly TargetIndex SeatIndex = TargetIndex.B;

	private Building Building => (Building)base.TargetThingA;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0, null, errorOnFailed))
		{
			return pawn.ReserveSittableOrSpot(job.targetB.Cell, job, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.EndOnDespawnedOrNull(BuildingIndex);
		yield return Toils_Goto.Goto(SeatIndex, PathEndMode.OnCell);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceTarget(Building);
			pawn.GainComfortFromCellIfPossible(delta);
			JoyUtility.JoyTickCheckEnd(pawn, delta, joySource: Building, fullJoyAction: job.doUntilGatheringEnded ? JoyTickFullJoyAction.None : JoyTickFullJoyAction.EndJob);
		};
		toil.handlingFacing = true;
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = (job.doUntilGatheringEnded ? job.expiryInterval : job.def.joyDuration);
		toil.AddFinishAction(delegate
		{
			JoyUtility.TryGainRecRoomThought(pawn);
		});
		ModifyPlayToil(toil);
		yield return toil;
	}

	protected virtual void ModifyPlayToil(Toil toil)
	{
	}

	public override object[] TaleParameters()
	{
		return new object[2]
		{
			pawn,
			base.TargetA.Thing.def
		};
	}
}
