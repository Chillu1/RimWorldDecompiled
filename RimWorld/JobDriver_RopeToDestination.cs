using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JobDriver_RopeToDestination : JobDriver
{
	private const TargetIndex AnimalInd = TargetIndex.A;

	public const TargetIndex DestCellInd = TargetIndex.B;

	private const int MaxAnimalsToRope = 10;

	private const int RopeMoreAnimalsScanRadius = 10;

	private static readonly List<Pawn> tmpRopees = new List<Pawn>();

	protected abstract bool HasRopeeArrived(Pawn ropee, bool roperWaitingAtDest);

	protected abstract void ProcessArrivedRopee(Pawn ropee);

	protected abstract bool ShouldOpportunisticallyRopeAnimal(Pawn animal);

	protected virtual Thing FindDistantAnimalToRope()
	{
		return null;
	}

	protected virtual bool UpdateDestination()
	{
		return false;
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.roping.Ropees.NullOrEmpty())
		{
			foreach (Pawn ropee in pawn.roping.Ropees)
			{
				pawn.Reserve(ropee, job, 1, -1, null, errorOnFailed);
			}
		}
		UpdateDestination();
		pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.GetTarget(TargetIndex.B).Cell);
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_General.Do(delegate
		{
			UpdateDestination();
		});
		AddFinishAction(delegate
		{
			pawn?.roping?.DropRopes();
		});
		Toil findAnotherAnimal = Toils_General.Label();
		Toil topOfLoop = Toils_General.Label();
		yield return topOfLoop;
		yield return Toils_Jump.JumpIf(findAnotherAnimal, () => (job.GetTarget(TargetIndex.A).Thing as Pawn)?.roping.RopedByPawn == pawn);
		yield return Toils_Reserve.Reserve(TargetIndex.A);
		yield return Toils_Rope.GotoRopeAttachmentInteractionCell(TargetIndex.A);
		yield return Toils_Rope.RopePawn(TargetIndex.A);
		yield return findAnotherAnimal;
		yield return Toils_Jump.JumpIf(topOfLoop, FindAnotherAnimalToRope);
		topOfLoop = Toils_General.Label();
		yield return topOfLoop;
		Toil toil = Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
		MatchLocomotionUrgency(toil);
		toil.AddPreTickIntervalAction(delegate
		{
			ProcessRopeesThatHaveArrived(roperWaitingAtDest: false);
		});
		toil.FailOn(() => !pawn.roping.IsRopingOthers);
		yield return toil;
		yield return Toils_Jump.JumpIf(topOfLoop, UpdateDestination);
		topOfLoop = Toils_General.Wait(60, TargetIndex.A);
		topOfLoop.AddPreTickIntervalAction(delegate
		{
			ProcessRopeesThatHaveArrived(roperWaitingAtDest: true);
		});
		yield return topOfLoop;
		yield return Toils_Jump.JumpIf(topOfLoop, () => pawn.roping.IsRopingOthers);
	}

	private void ProcessRopeesThatHaveArrived(bool roperWaitingAtDest)
	{
		tmpRopees.Clear();
		tmpRopees.AddRange(pawn.roping.Ropees);
		foreach (Pawn tmpRopee in tmpRopees)
		{
			if (HasRopeeArrived(tmpRopee, roperWaitingAtDest))
			{
				pawn.roping.DropRope(tmpRopee);
				if (tmpRopee.jobs != null && tmpRopee.CurJob != null && tmpRopee.jobs.curDriver is JobDriver_FollowRoper)
				{
					tmpRopee.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				ProcessArrivedRopee(tmpRopee);
			}
		}
	}

	private void MatchLocomotionUrgency(Toil toil)
	{
		toil.AddPreInitAction(delegate
		{
			locomotionUrgencySameAs = SlowestRopee();
		});
		toil.AddFinishAction(delegate
		{
			locomotionUrgencySameAs = null;
		});
	}

	private Pawn SlowestRopee()
	{
		if (!pawn.roping.Ropees.TryMaxBy((Pawn p) => p.TicksPerMoveCardinal, out var value))
		{
			return null;
		}
		return value;
	}

	private bool FindAnotherAnimalToRope()
	{
		int num = pawn.mindState?.duty?.ropeeLimit ?? 10;
		if (pawn.roping.Ropees.Count >= num)
		{
			return false;
		}
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.Touch, TraverseParms.For(pawn), 10f, AnimalValidator);
		if (thing == null)
		{
			thing = FindDistantAnimalToRope();
		}
		if (thing != null)
		{
			job.SetTarget(TargetIndex.A, thing);
			return true;
		}
		return false;
		bool AnimalValidator(Thing thing2)
		{
			if (!(thing2 is Pawn animal))
			{
				return false;
			}
			return ShouldOpportunisticallyRopeAnimal(animal);
		}
	}
}
