using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JobDriver_InteractAnimal : JobDriver
{
	protected const TargetIndex AnimalInd = TargetIndex.A;

	private const TargetIndex FoodHandInd = TargetIndex.B;

	private const int FeedDuration = 270;

	private const int TalkDuration = 270;

	private const float NutritionPercentagePerFeed = 0.15f;

	private const float MaxMinNutritionPerFeed = 0.3f;

	public const int FeedCount = 2;

	public const FoodPreferability MaxFoodPreferability = FoodPreferability.RawTasty;

	private float feedNutritionLeft;

	protected Pawn Animal => (Pawn)job.targetA.Thing;

	protected virtual bool CanInteractNow => true;

	protected virtual bool CanFeedEver => Animal?.needs?.food != null;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref feedNutritionLeft, "feedNutritionLeft", 0f);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Animal, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOnDowned(TargetIndex.A);
		this.FailOnNotCasualInterruptible(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return TalkToAnimal(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return TalkToAnimal(TargetIndex.A);
		if (CanFeedEver)
		{
			foreach (Toil item in FeedToils())
			{
				yield return item;
			}
		}
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
		yield return TalkToAnimal(TargetIndex.A);
		if (CanFeedEver)
		{
			foreach (Toil item2 in FeedToils())
			{
				yield return item2;
			}
		}
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !CanInteractNow);
		yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
		yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
		yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
	}

	public static float RequiredNutritionPerFeed(Pawn animal)
	{
		if (animal.needs.food == null)
		{
			return 0f;
		}
		return Mathf.Min(animal.needs.food.MaxLevel * 0.15f, 0.3f);
	}

	private IEnumerable<Toil> FeedToils()
	{
		Toil toil = ToilMaker.MakeToil("FeedToils");
		toil.initAction = delegate
		{
			feedNutritionLeft = RequiredNutritionPerFeed(Animal);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
		Toil gotoAnimal = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return gotoAnimal;
		yield return StartFeedAnimal(TargetIndex.A);
		yield return Toils_Ingest.FinalizeIngest(Animal, TargetIndex.B);
		yield return Toils_General.PutCarriedThingInInventory();
		yield return Toils_General.ClearTarget(TargetIndex.B);
		yield return Toils_Jump.JumpIf(gotoAnimal, () => feedNutritionLeft > 0f);
	}

	private Toil TalkToAnimal(TargetIndex tameeInd)
	{
		Toil toil = ToilMaker.MakeToil("TalkToAnimal");
		toil.initAction = delegate
		{
			Pawn actor = toil.GetActor();
			Pawn recipient = (Pawn)(Thing)actor.CurJob.GetTarget(tameeInd);
			actor.interactions.TryInteractWith(recipient, InteractionDefOf.AnimalChat);
		};
		toil.FailOn(() => !CanInteractNow);
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 270;
		toil.activeSkill = () => SkillDefOf.Animals;
		return toil;
	}

	private Toil StartFeedAnimal(TargetIndex tameeInd)
	{
		Toil toil = ToilMaker.MakeToil("StartFeedAnimal");
		toil.initAction = delegate
		{
			Pawn actor = toil.GetActor();
			Pawn pawn = (Pawn)(Thing)actor.CurJob.GetTarget(tameeInd);
			PawnUtility.ForceWait(pawn, 270, actor);
			Thing thing = FoodUtility.BestFoodInInventory(actor, pawn, FoodPreferability.NeverForNutrition, FoodPreferability.RawTasty);
			if (thing == null)
			{
				actor.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
			else
			{
				actor.mindState.lastInventoryRawFoodUseTick = Find.TickManager.TicksGame;
				int num = FoodUtility.StackCountForNutrition(feedNutritionLeft, thing.GetStatValue(StatDefOf.Nutrition));
				int stackCount = thing.stackCount;
				Thing thing2 = actor.inventory.innerContainer.Take(thing, Mathf.Min(num, stackCount));
				actor.carryTracker.TryStartCarry(thing2);
				actor.CurJob.SetTarget(TargetIndex.B, thing2);
				float num2 = (float)thing2.stackCount * thing2.GetStatValue(StatDefOf.Nutrition);
				ticksLeftThisToil = Mathf.CeilToInt(270f * (num2 / RequiredNutritionPerFeed(pawn)));
				if (num <= stackCount)
				{
					feedNutritionLeft = 0f;
				}
				else
				{
					feedNutritionLeft -= num2;
					if (feedNutritionLeft < 0.001f)
					{
						feedNutritionLeft = 0f;
					}
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.activeSkill = () => SkillDefOf.Animals;
		return toil;
	}
}
