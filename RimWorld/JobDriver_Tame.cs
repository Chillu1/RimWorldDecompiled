using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Tame : JobDriver_InteractAnimal
{
	private const TargetIndex FoodIndex = TargetIndex.C;

	protected override bool CanInteractNow => !TameUtility.TriedToTameTooRecently(base.Animal);

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Func<bool> noLongerDesignated = () => base.Map.designationManager.DesignationOn(base.Animal, DesignationDefOf.Tame) == null;
		if (job.GetTarget(TargetIndex.C).HasThing)
		{
			yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.C).FailOn(noLongerDesignated);
			yield return Toils_Haul.TakeToInventory(TargetIndex.C, job.count).FailOn(noLongerDesignated);
		}
		foreach (Toil item in base.MakeNewToils())
		{
			item.FailOn(noLongerDesignated);
			yield return item;
		}
		yield return Toils_Interpersonal.TryRecruit(TargetIndex.A);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (base.job.GetTarget(TargetIndex.A).Thing is Pawn pawn && AnimalPenUtility.NeedsToBeManagedByRope(pawn) && pawn.Faction == Faction.OfPlayer && AnimalPenUtility.GetCurrentPenOf(pawn, allowUnenclosedPens: false) == null)
			{
				RopingPriority ropingPriority = RopingPriority.Closest;
				string jobFailReason;
				CompAnimalPenMarker penAnimalShouldBeTakenTo = AnimalPenUtility.GetPenAnimalShouldBeTakenTo(base.pawn, pawn, out jobFailReason, forced: false, canInteractWhileSleeping: true, allowUnenclosedPens: true, ignoreSkillRequirements: true, ropingPriority);
				Job job = null;
				if (penAnimalShouldBeTakenTo != null)
				{
					job = WorkGiver_TakeToPen.MakeJob(base.pawn, pawn, penAnimalShouldBeTakenTo, allowUnenclosedPens: true, ropingPriority, out jobFailReason);
				}
				if (job != null)
				{
					base.pawn.jobs.StartJob(job, JobCondition.Succeeded);
				}
				else
				{
					Messages.Message("MessageTameNoSuitablePens".Translate(pawn.Named("ANIMAL")), pawn, MessageTypeDefOf.NeutralEvent);
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
	}
}
