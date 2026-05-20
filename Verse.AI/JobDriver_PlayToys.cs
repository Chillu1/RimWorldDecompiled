using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI;

public class JobDriver_PlayToys : JobDriver_BabyPlay
{
	private const TargetIndex ToyBoxInd = TargetIndex.B;

	private const int ToysCount = 5;

	private const float ToyDistanceFactor = 0.5f;

	private static readonly FloatRange ToyRandomAngleOffset = new FloatRange(-5f, 5f);

	private const int InteractionTicks = 600;

	private Mote[] motesToMaintain;

	private Thing ToyBox => base.TargetThingB;

	private Building_Bed Bed => (Building_Bed)base.TargetThingC;

	protected override StartingConditions StartingCondition => StartingConditions.PickupBaby;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!base.TryMakePreToilReservations(errorOnFailed))
		{
			return false;
		}
		if (Bed != null && !pawn.Reserve(Bed, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> Play()
	{
		this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
		this.FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.Touch);
		yield return DropBaby();
		yield return PlayToil();
	}

	private Toil PlayToil()
	{
		Toil toil = ToilMaker.MakeToil("PlayToil");
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.handlingFacing = true;
		toil.WithEffect(EffecterDefOf.PlayStatic, TargetIndex.A);
		toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, (Action<int>)delegate(int delta)
		{
			if (motesToMaintain.NullOrEmpty())
			{
				Vector3 vector = base.Baby.TrueCenter();
				Vector3 v = IntVec3.North.ToVector3();
				float num = 72f;
				motesToMaintain = new Mote[5];
				for (int i = 0; i < 5; i++)
				{
					Vector3 loc = vector + v.RotatedBy(num * (float)i + ToyRandomAngleOffset.RandomInRange) * 0.5f;
					motesToMaintain[i] = MoteMaker.MakeStaticMote(loc, base.Map, ThingDefOf.Mote_Toy);
				}
			}
			pawn.rotationTracker.FaceTarget(base.Baby);
			for (int j = 0; j < motesToMaintain.Length; j++)
			{
				motesToMaintain[j]?.Maintain();
			}
			if (pawn.IsHashIntervalTick(600, delta))
			{
				pawn.interactions.TryInteractWith(base.Baby, InteractionDefOf.BabyPlay);
			}
			if (roomPlayGainFactor < 0f)
			{
				roomPlayGainFactor = BabyPlayUtility.GetRoomPlayGainFactors(base.Baby);
			}
			if (BabyPlayUtility.PlayTickCheckEnd(base.Baby, pawn, roomPlayGainFactor, delta, ToyBox))
			{
				pawn.jobs.curDriver.ReadyForNextToil();
			}
		});
		ChildcareUtility.MakeBabyPlayAsLongAsToilIsActive(toil, TargetIndex.A);
		return toil;
	}

	private Toil DropBaby()
	{
		Toil toil = ToilMaker.MakeToil("DropBaby");
		toil.initAction = delegate
		{
			pawn.carryTracker.innerContainer.TryDrop(base.Baby, ThingPlaceMode.Near, 1, out var _, null, (IntVec3 c) => !ToyBox.OccupiedRect().Contains(c));
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}
}
