using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobDriver_ReturnedCaravan_PenAnimals : JobDriver_RopeToDestination
{
	public const TargetIndex DestPenInd = TargetIndex.C;

	private List<Pawn> tmpRopees = new List<Pawn>();

	protected override bool UpdateDestination()
	{
		tmpRopees.Clear();
		tmpRopees.AddRange(pawn.roping.Ropees);
		try
		{
			IntVec3 cell = job.GetTarget(TargetIndex.B).Cell;
			District district = (cell.IsValid ? cell.GetDistrict(base.Map) : null);
			IntVec3 intVec = IntVec3.Invalid;
			Thing thing = null;
			foreach (Pawn tmpRopee in tmpRopees)
			{
				string jobFailReason;
				CompAnimalPenMarker penAnimalShouldBeTakenTo = AnimalPenUtility.GetPenAnimalShouldBeTakenTo(pawn, tmpRopee, out jobFailReason, forced: false, canInteractWhileSleeping: true, allowUnenclosedPens: true);
				if (penAnimalShouldBeTakenTo == null)
				{
					pawn.roping.DropRope(tmpRopee);
					continue;
				}
				if (penAnimalShouldBeTakenTo.parent.GetDistrict() == district)
				{
					return false;
				}
				if (!intVec.IsValid)
				{
					thing = penAnimalShouldBeTakenTo.parent;
					intVec = penAnimalShouldBeTakenTo.parent.Position;
				}
			}
			if (intVec.IsValid)
			{
				job.SetTarget(TargetIndex.B, intVec);
				job.SetTarget(TargetIndex.C, thing);
				return true;
			}
			return false;
		}
		finally
		{
			tmpRopees.Clear();
		}
	}

	private bool PenIsEnclosed()
	{
		LocalTargetInfo target = job.GetTarget(TargetIndex.C);
		if (!target.HasThing)
		{
			return false;
		}
		if (!(target.Thing is ThingWithComps thingWithComps))
		{
			return false;
		}
		return thingWithComps.GetComp<CompAnimalPenMarker>()?.PenState.Enclosed ?? false;
	}

	protected override bool HasRopeeArrived(Pawn ropee, bool roperWaitingAtDest)
	{
		if (job.ropeToUnenclosedPens && !PenIsEnclosed())
		{
			return roperWaitingAtDest;
		}
		return AnimalPenUtility.GetCurrentPenOf(ropee, job.ropeToUnenclosedPens) != null;
	}

	protected override void ProcessArrivedRopee(Pawn ropee)
	{
		ropee.GetLord()?.Notify_PawnLost(ropee, PawnLostCondition.LeftVoluntarily);
	}

	protected override bool ShouldOpportunisticallyRopeAnimal(Pawn animal)
	{
		return false;
	}
}
