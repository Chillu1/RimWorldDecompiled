using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_WatchBuilding : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0, null, errorOnFailed))
		{
			return false;
		}
		if (!pawn.ReserveSittableOrSpot(job.targetB.Cell, job, errorOnFailed))
		{
			return false;
		}
		if (base.TargetC.HasThing && base.TargetC.Thing is Building_Bed && !pawn.Reserve(job.targetC, job, ((Building_Bed)base.TargetC.Thing).SleepingSlotsCount, 0, null, errorOnFailed))
		{
			return false;
		}
		return true;
	}

	public override bool CanBeginNowWhileLyingDown()
	{
		if (base.TargetC.HasThing && base.TargetC.Thing is Building_Bed)
		{
			return JobInBedUtility.InBedOrRestSpotNow(pawn, base.TargetC);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.EndOnDespawnedOrNull(TargetIndex.A);
		Toil watch;
		if (base.TargetC.HasThing && base.TargetC.Thing is Building_Bed)
		{
			this.KeepLyingDown(TargetIndex.C);
			yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.C);
			yield return Toils_Bed.GotoBed(TargetIndex.C);
			watch = Toils_LayDown.LayDown(TargetIndex.C, hasBed: true, lookForOtherJobs: false);
			watch.AddFailCondition(() => !watch.actor.Awake());
		}
		else
		{
			yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			watch = ToilMaker.MakeToil("MakeNewToils");
		}
		watch.AddPreTickIntervalAction(WatchTickAction);
		watch.AddFinishAction(delegate
		{
			JoyUtility.TryGainRecRoomThought(pawn);
		});
		watch.defaultCompleteMode = ToilCompleteMode.Delay;
		watch.defaultDuration = job.def.joyDuration;
		watch.handlingFacing = true;
		if (base.TargetA.Thing.def.building != null && base.TargetA.Thing.def.building.effectWatching != null)
		{
			watch.WithEffect(() => base.TargetA.Thing.def.building.effectWatching, EffectTargetGetter);
		}
		yield return watch;
		LocalTargetInfo EffectTargetGetter()
		{
			return base.TargetA.Thing.OccupiedRect().RandomCell + IntVec3.North.RotatedBy(base.TargetA.Thing.Rotation);
		}
	}

	protected virtual void WatchTickAction(int delta)
	{
		pawn.rotationTracker.FaceCell(base.TargetA.Cell);
		pawn.GainComfortFromCellIfPossible(delta);
		JoyUtility.JoyTickCheckEnd(pawn, delta, JoyTickFullJoyAction.EndJob, 1f, (Building)base.TargetThingA);
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
