using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JobDriver_FeedBaby : JobDriver
{
	public const float BabyFedMemoryFoodNeedIncrease = 0.6f;

	protected const TargetIndex BabyInd = TargetIndex.A;

	private const int ChairSearchRadius = 32;

	protected float initialFoodPercentage;

	protected Pawn Baby => (Pawn)base.TargetThingA;

	protected abstract Toil FeedingToil { get; set; }

	public bool Feeding => base.CurToil == FeedingToil;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Baby, job, 1, -1, null, errorOnFailed);
	}

	protected abstract IEnumerable<Toil> FeedBaby();

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		AddFailCondition(() => !ChildcareUtility.CanSuckle(Baby, out var _));
		SetFinalizerJob((JobCondition _) => (!pawn.IsCarryingPawn(Baby)) ? null : ChildcareUtility.MakeBringBabyToSafetyJob(pawn, Baby));
		IEnumerator<Toil> feedBabyEnumerator = FeedBaby().GetEnumerator();
		if (!feedBabyEnumerator.MoveNext())
		{
			throw new InvalidOperationException("There must be at least one toil in FeedBaby.");
		}
		Toil firstFeedingToil = feedBabyEnumerator.Current;
		Toil jumpIfDownedOrDrafted = Toils_Jump.JumpIf(firstFeedingToil, () => pawn.Downed || pawn.Drafted).FailOn(() => !pawn.IsCarryingPawn(Baby));
		yield return Toils_Jump.JumpIf(jumpIfDownedOrDrafted, () => pawn.IsCarryingPawn(Baby)).FailOn(() => !pawn.IsCarryingPawn(Baby) && (pawn.Downed || pawn.Drafted));
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A);
		yield return jumpIfDownedOrDrafted;
		yield return GoToChair();
		yield return SitInChair();
		yield return firstFeedingToil;
		while (feedBabyEnumerator.MoveNext())
		{
			yield return feedBabyEnumerator.Current;
		}
		feedBabyEnumerator.Dispose();
	}

	private Toil GoToChair()
	{
		Toil toil = ToilMaker.MakeToil("GoToChair");
		toil.initAction = delegate
		{
			IntVec3 breastfeedSpot = RCellFinder.SpotToStandDuringJob(pawn);
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(pawn), 32f, (Thing t) => IsValidBreastfeedChair(t) && (int)t.Position.GetDangerFor(pawn, t.Map) <= (int)breastfeedSpot.GetDangerFor(pawn, pawn.Map));
			if (thing != null)
			{
				Toils_Ingest.TryFindFreeSittingSpotOnThing(thing, pawn, out breastfeedSpot);
			}
			if (!breastfeedSpot.IsValid)
			{
				pawn.pather.StartPath(pawn.Position, PathEndMode.OnCell);
			}
			else
			{
				pawn.ReserveSittableOrSpot(breastfeedSpot, toil.actor.CurJob);
				pawn.Map.pawnDestinationReservationManager.Reserve(pawn, pawn.CurJob, breastfeedSpot);
				pawn.pather.StartPath(breastfeedSpot, PathEndMode.OnCell);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		return toil;
	}

	private Toil SitInChair()
	{
		Toil toil = ToilMaker.MakeToil("SitInChair");
		toil.initAction = delegate
		{
			if (pawn.Spawned)
			{
				Thing thing = pawn.Position.GetThingList(pawn.Map).FirstOrDefault(IsValidBreastfeedChair);
				if (thing != null)
				{
					pawn.Rotation = thing.Rotation;
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

	private bool IsValidBreastfeedChair(Thing t)
	{
		if (t.def.building == null || !t.def.building.isSittable)
		{
			return false;
		}
		if (!Toils_Ingest.TryFindFreeSittingSpotOnThing(t, pawn, out var _))
		{
			return false;
		}
		if (t.IsForbidden(pawn))
		{
			return false;
		}
		if (!pawn.CanReserve(t))
		{
			return false;
		}
		if (!t.IsSociallyProper(pawn))
		{
			return false;
		}
		if (t.IsBurning())
		{
			return false;
		}
		if (t.HostileTo(pawn))
		{
			return false;
		}
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref initialFoodPercentage, "initialFoodPercentage", 0f);
	}
}
