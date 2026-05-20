using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI;

public class JobDriver_PlayWalking : JobDriver_BabyPlay
{
	private const TargetIndex WanderCellInd = TargetIndex.B;

	private const int InteractionTicks = 600;

	private const int InteractionIntervalTicks = 300;

	private const float WanderRadius = 8f;

	protected override StartingConditions StartingCondition => StartingConditions.PickupBaby;

	public override Vector3 ForcedBodyOffset
	{
		get
		{
			if (!pawn.IsCarryingPawn(base.Baby))
			{
				return Vector3.zero;
			}
			float f = (float)Find.TickManager.TicksGame / 60f * 2f;
			if (pawn.pather.Moving)
			{
				float f2 = Mathf.Sin(f);
				float num = Mathf.Sign(f2);
				return new Vector3(EasingFunctions.EaseInOutQuad(Mathf.Abs(f2) * 0.6f) * num * 0.12f, 0f, 0f);
			}
			float z = EasingFunctions.EaseInOutQuint(Mathf.Abs(Mathf.Sin(f) * 0.6f)) * 0.12f;
			return new Vector3(0f, 0f, z);
		}
	}

	public static IntVec3 TryFindWanderCell(Pawn pawn, IntVec3 root)
	{
		return RCellFinder.RandomWanderDestFor(pawn, root, 8f, null, PawnUtility.ResolveMaxDanger(pawn, Danger.None));
	}

	private void CheckEndJobSuccess(int delta)
	{
		if (roomPlayGainFactor < 0f || pawn.IsHashIntervalTick(300, delta))
		{
			roomPlayGainFactor = BabyPlayUtility.GetRoomPlayGainFactors(base.Baby);
		}
		if (BabyPlayUtility.PlayTickCheckEnd(base.Baby, pawn, roomPlayGainFactor, delta))
		{
			pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
		}
	}

	protected override IEnumerable<Toil> Play()
	{
		Toil playWalking = PlayWalking();
		yield return playWalking;
		yield return Interact();
		yield return Toils_Jump.JumpIf(playWalking, delegate
		{
			IntVec3 intVec = TryFindWanderCell(pawn, pawn.Position);
			if (intVec.IsValid)
			{
				job.SetTarget(TargetIndex.B, intVec);
				return true;
			}
			return false;
		});
	}

	private Toil PlayWalking()
	{
		Toil toil = Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
		toil.initAction = (Action)Delegate.Combine(toil.initAction, (Action)delegate
		{
			job.locomotionUrgency = LocomotionUrgency.Amble;
		});
		toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, new Action<int>(CheckEndJobSuccess));
		ChildcareUtility.MakeBabyPlayAsLongAsToilIsActive(toil, TargetIndex.A);
		return toil;
	}

	private Toil Interact()
	{
		Toil toil = Toils_General.Wait(600);
		toil.WithEffect(EffecterDefOf.PlayStatic, TargetIndex.A);
		toil.initAction = (Action)Delegate.Combine(toil.initAction, (Action)delegate
		{
			pawn.Rotation = Rot4.Random;
		});
		toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, (Action<int>)delegate(int delta)
		{
			if (pawn.IsHashIntervalTick(300, delta))
			{
				pawn.interactions.TryInteractWith(base.Baby, InteractionDefOf.BabyPlay);
			}
			CheckEndJobSuccess(delta);
		});
		ChildcareUtility.MakeBabyPlayAsLongAsToilIsActive(toil, TargetIndex.A);
		return toil;
	}
}
