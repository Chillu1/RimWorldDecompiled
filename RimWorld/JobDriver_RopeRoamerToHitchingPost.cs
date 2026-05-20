using Verse;

namespace RimWorld
{
	public class JobDriver_RopeRoamerToHitchingPost : JobDriver_RopeToDestination
	{
		protected Building HitchingPost => base.TargetThingB as Building;

		protected IntVec3 HitchingPostLoc => HitchingPost.Position;

		protected override bool HasRopeeArrived(Pawn ropee, bool roperWaitingAtDest)
		{
			if (!HitchingPostLoc.IsValid)
			{
				return false;
			}
			if (!pawn.Position.InHorDistOf(HitchingPostLoc, 2f))
			{
				return false;
			}
			District district = HitchingPostLoc.GetDistrict(pawn.Map);
			if (district != pawn.GetDistrict() || district != ropee.GetDistrict())
			{
				return false;
			}
			return true;
		}

		protected override void ProcessArrivedRopee(Pawn ropee)
		{
			if (HitchingPostLoc.IsValid)
			{
				ropee.roping.RopeToSpot(HitchingPostLoc);
			}
		}

		protected override bool ShouldOpportunisticallyRopeAnimal(Pawn animal)
		{
			if (animal.roping.RopedByPawn == pawn)
			{
				return false;
			}
			string jobFailReason;
			Building hitchingPostAnimalShouldBeTakenTo = AnimalPenUtility.GetHitchingPostAnimalShouldBeTakenTo(pawn, animal, out jobFailReason);
			if (hitchingPostAnimalShouldBeTakenTo != null && HitchingPost == hitchingPostAnimalShouldBeTakenTo)
			{
				return AnimalPenUtility.GetPenAnimalShouldBeTakenTo(pawn, animal, out jobFailReason, forced: false, canInteractWhileSleeping: true, allowUnenclosedPens: false, ignoreSkillRequirements: true, job.ropingPriority) == null;
			}
			return false;
		}
	}
}
