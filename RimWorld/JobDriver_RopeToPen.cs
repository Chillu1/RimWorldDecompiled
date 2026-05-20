using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RopeToPen : JobDriver_RopeToDestination
	{
		public const TargetIndex DestMarkerInd = TargetIndex.C;

		private Thing DestinationThing => job.targetC.Thing;

		protected CompAnimalPenMarker DestinationMarker => DestinationThing.TryGetComp<CompAnimalPenMarker>();

		private District DestinationDistrict => DestinationThing.GetDistrict();

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.C);
			return base.MakeNewToils();
		}

		protected override bool HasRopeeArrived(Pawn ropee, bool roperWaitingAtDest)
		{
			PenMarkerState penState = DestinationMarker.PenState;
			if (penState.Enclosed)
			{
				return penState.ContainsConnectedRegion(ropee.GetRegion());
			}
			return true;
		}

		protected override void ProcessArrivedRopee(Pawn ropee)
		{
		}

		protected override bool ShouldOpportunisticallyRopeAnimal(Pawn animal)
		{
			if (animal.roping.RopedByPawn == pawn)
			{
				return false;
			}
			string jobFailReason;
			CompAnimalPenMarker penAnimalShouldBeTakenTo = AnimalPenUtility.GetPenAnimalShouldBeTakenTo(pawn, animal, out jobFailReason, forced: false, canInteractWhileSleeping: true, job.ropeToUnenclosedPens, ignoreSkillRequirements: true, job.ropingPriority);
			if (penAnimalShouldBeTakenTo == null)
			{
				return false;
			}
			return ShouldOpportunisticallyRopeAnimal(animal, penAnimalShouldBeTakenTo);
		}

		protected virtual bool ShouldOpportunisticallyRopeAnimal(Pawn animal, CompAnimalPenMarker targetPenMarker)
		{
			return targetPenMarker.parent.GetDistrict() == DestinationDistrict;
		}
	}
}
