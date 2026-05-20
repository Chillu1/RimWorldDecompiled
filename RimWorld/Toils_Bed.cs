using Verse;
using Verse.AI;

namespace RimWorld;

public static class Toils_Bed
{
	public static Toil GotoBed(TargetIndex bedIndex)
	{
		Toil gotoBed = ToilMaker.MakeToil("GotoBed");
		gotoBed.initAction = delegate
		{
			Pawn actor = gotoBed.actor;
			Building_Bed bed = (Building_Bed)actor.CurJob.GetTarget(bedIndex).Thing;
			IntVec3 bedSleepingSlotPosFor = RestUtility.GetBedSleepingSlotPosFor(actor, bed);
			if (actor.Position == bedSleepingSlotPosFor)
			{
				actor.jobs.curDriver.ReadyForNextToil();
			}
			else
			{
				actor.pather.StartPath(bedSleepingSlotPosFor, PathEndMode.OnCell);
			}
		};
		gotoBed.tickIntervalAction = delegate
		{
			Pawn actor = gotoBed.actor;
			Building_Bed building_Bed = (Building_Bed)actor.CurJob.GetTarget(bedIndex).Thing;
			Pawn curOccupantAt = building_Bed.GetCurOccupantAt(actor.pather.Destination.Cell);
			if (curOccupantAt != null && curOccupantAt != actor)
			{
				actor.pather.StartPath(RestUtility.GetBedSleepingSlotPosFor(actor, building_Bed), PathEndMode.OnCell);
			}
		};
		gotoBed.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		gotoBed.FailOnBedNoLongerUsable(bedIndex);
		return gotoBed;
	}

	public static Toil ClaimBedIfNonMedical(TargetIndex ind, TargetIndex claimantIndex = TargetIndex.None)
	{
		Toil claim = ToilMaker.MakeToil("ClaimBedIfNonMedical");
		claim.initAction = delegate
		{
			Pawn actor = claim.GetActor();
			Pawn pawn = ((claimantIndex == TargetIndex.None) ? actor : ((Pawn)actor.CurJob.GetTarget(claimantIndex).Thing));
			if (pawn.ownership != null && (!pawn.IsMutant || !pawn.mutant.Def.disableOwnership))
			{
				pawn.ownership.ClaimBedIfNonMedical((Building_Bed)actor.CurJob.GetTarget(ind).Thing);
			}
		};
		claim.FailOnDespawnedOrNull(ind);
		claim.defaultCompleteMode = ToilCompleteMode.Instant;
		claim.atomicWithPrevious = true;
		return claim;
	}

	public static bool BedNoLongerUsable(Pawn actor, Thing bedThing, bool forcePrisoner)
	{
		GuestStatus? guestStatusOverride = null;
		if (forcePrisoner)
		{
			guestStatusOverride = GuestStatus.Prisoner;
		}
		return !RestUtility.CanUseBedNow(bedThing, actor, checkSocialProperness: false, allowMedBedEvenIfSetToNoCare: false, guestStatusOverride);
	}

	private static Pawn TargetIndexToPawn(Toil toil, TargetIndex pawnIndex)
	{
		if (pawnIndex == TargetIndex.None)
		{
			return toil.actor;
		}
		return (Pawn)toil.actor.CurJob.GetTarget(pawnIndex).Thing;
	}

	public static void FailOnBedNoLongerUsable(this Toil toil, TargetIndex bedIndex)
	{
		toil.FailOnBedNoLongerUsable(bedIndex, TargetIndex.None);
	}

	public static void FailOnBedNoLongerUsable(this Toil toil, TargetIndex bedIndex, TargetIndex sleeperIndex)
	{
		toil.FailOn(() => BedNoLongerUsable(TargetIndexToPawn(toil, sleeperIndex), toil.actor.CurJob.GetTarget(bedIndex).Thing, toil.actor.CurJobDef.makeTargetPrisoner));
		toil.AddFinishAction(delegate
		{
			Building_Bed building_Bed = (Building_Bed)toil.actor.CurJob.GetTarget(bedIndex).Thing;
			Pawn pawn = TargetIndexToPawn(toil, sleeperIndex);
			if (pawn.GetPosture().InBed() && BedNoLongerUsable(pawn, building_Bed, toil.actor.CurJobDef.makeTargetPrisoner))
			{
				RestUtility.KickOutOfBed(pawn, building_Bed);
			}
		});
	}

	public static Toil TuckIntoBed(Building_Bed bed, Pawn taker, Pawn takee, bool rescued = false)
	{
		Toil toil = ToilMaker.MakeToil("TuckIntoBed");
		toil.initAction = delegate
		{
			RestUtility.TuckIntoBed(bed, taker, takee, rescued);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

	public static Toil TuckIntoBed(TargetIndex bedIndex, TargetIndex takeeIndex, bool rescued = false)
	{
		Toil tuckIntoBed = ToilMaker.MakeToil("TuckIntoBed");
		tuckIntoBed.initAction = delegate
		{
			Pawn actor = tuckIntoBed.actor;
			Building_Bed building_Bed = actor.jobs.curJob.GetTarget(bedIndex).Thing as Building_Bed;
			Pawn takee = (Pawn)actor.jobs.curJob.GetTarget(takeeIndex).Thing;
			if (building_Bed != null)
			{
				RestUtility.TuckIntoBed(building_Bed, actor, takee, rescued);
			}
			else
			{
				actor.carryTracker.TryDropCarriedThing(actor.jobs.curJob.GetTarget(bedIndex).Cell, ThingPlaceMode.Direct, out var _);
			}
		};
		tuckIntoBed.defaultCompleteMode = ToilCompleteMode.Instant;
		return tuckIntoBed;
	}
}
