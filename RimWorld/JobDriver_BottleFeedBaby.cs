using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_BottleFeedBaby : JobDriver_FeedBaby
{
	protected const TargetIndex BabyFoodInd = TargetIndex.B;

	private float bottleNutrition;

	private float totalBottleNutrition;

	private float initialNutritionNeeded;

	protected Thing BabyFood => base.TargetThingB;

	protected LocalTargetInfo BabyFoodTarget => base.TargetB;

	protected override Toil FeedingToil { get; set; }

	protected override IEnumerable<Toil> MakeNewToils()
	{
		AddFailCondition(() => !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
		Toil failIfNoBabyFoodInInventory = FailIfNoBabyFoodInInventory();
		yield return Toils_Jump.JumpIf(failIfNoBabyFoodInInventory, () => !BabyFoodTarget.IsValid || pawn.inventory.Contains(BabyFood));
		yield return Toils_Ingest.ReserveFoodFromStackForIngesting(TargetIndex.B, base.Baby).FailOnDestroyedNullOrForbidden(TargetIndex.B);
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDestroyedNullOrForbidden(TargetIndex.B);
		yield return Toils_Haul.TakeToInventory(TargetIndex.B, delegate(Thing babyFood)
		{
			int b = FoodUtility.WillIngestStackCountOf(base.Baby, babyFood.def, FoodUtility.NutritionForEater(base.Baby, babyFood));
			return Mathf.Min(babyFood.stackCount, b);
		}).FailOnDestroyedNullOrForbidden(TargetIndex.B);
		yield return failIfNoBabyFoodInInventory;
		foreach (Toil item in base.MakeNewToils())
		{
			yield return item;
		}
	}

	private Toil FailIfNoBabyFoodInInventory()
	{
		Toil toil = ToilMaker.MakeToil("FailIfNoBabyFoodInInventory");
		toil.FailOn(() => FoodUtility.BestFoodInInventory(pawn, base.Baby) == null);
		return toil;
	}

	protected override IEnumerable<Toil> FeedBaby()
	{
		yield return FeedBabyFoodFromInventory();
	}

	private Toil FeedBabyFoodFromInventory()
	{
		FeedingToil = ToilMaker.MakeToil("FeedBabyFoodFromInventory");
		FeedingToil.initAction = delegate
		{
			initialNutritionNeeded = base.Baby.needs.food.NutritionWanted;
			initialFoodPercentage = base.Baby.needs.food.CurLevelPercentage;
			base.Baby.jobs.StartJob(ChildcareUtility.MakeBabySuckleJob(pawn), JobCondition.InterruptForced);
		};
		FeedingToil.tickIntervalAction = delegate(int delta)
		{
			float num = base.Baby.needs.food.MaxLevel / 5000f;
			float num2 = Mathf.Min(base.Baby.needs.food.NutritionWanted, num * (float)delta);
			bottleNutrition += num2;
			totalBottleNutrition += num2;
			pawn.GainComfortFromCellIfPossible(delta);
			base.Baby.ideo?.IncreaseIdeoExposureIfBabyTick(pawn.Ideo);
			if (!pawn.Downed && pawn.Rotation == Rot4.North)
			{
				pawn.Rotation = Rot4.East;
			}
			while (true)
			{
				Thing thing = FoodUtility.BestFoodInInventory(pawn, base.Baby);
				if (thing == null)
				{
					ReadyForNextToil();
					break;
				}
				if (bottleNutrition >= base.Baby.needs.food.NutritionWanted)
				{
					float num3 = thing.Ingested(base.Baby, bottleNutrition);
					base.Baby.records.AddTo(RecordDefOf.NutritionEaten, num3);
					bottleNutrition -= num3;
					base.Baby.needs.food.CurLevel = Mathf.Clamp(base.Baby.needs.food.CurLevel + num3, 0f, base.Baby.needs.food.MaxLevel);
					if (base.Baby.needs.food.CurLevel >= base.Baby.needs.food.MaxLevel)
					{
						ReadyForNextToil();
						break;
					}
				}
				else
				{
					float num4 = FoodUtility.NutritionForEater(base.Baby, thing);
					if (!(bottleNutrition >= num4))
					{
						break;
					}
					float num5 = thing.Ingested(base.Baby, num4);
					base.Baby.records.AddTo(RecordDefOf.NutritionEaten, num5);
					bottleNutrition -= num5;
					base.Baby.needs.food.CurLevel = Mathf.Clamp(base.Baby.needs.food.CurLevel + num5, 0f, base.Baby.needs.food.MaxLevel);
				}
			}
		};
		FeedingToil.AddFinishAction(delegate
		{
			if (base.Baby.needs != null && base.Baby.needs.food.CurLevelPercentage - initialFoodPercentage > 0.6f)
			{
				base.Baby.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FedMe, pawn);
				pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FedBaby, base.Baby);
			}
			if (base.Baby.CurJobDef == JobDefOf.BabySuckle)
			{
				base.Baby.jobs.EndCurrentJob(JobCondition.Succeeded);
			}
		});
		FeedingToil.handlingFacing = true;
		FeedingToil.WithProgressBar(TargetIndex.A, () => totalBottleNutrition / initialNutritionNeeded);
		FeedingToil.defaultCompleteMode = ToilCompleteMode.Never;
		FeedingToil.WithEffect(EffecterDefOf.Breastfeeding, TargetIndex.A);
		return FeedingToil;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref bottleNutrition, "bottleNutrition", 0f);
		Scribe_Values.Look(ref totalBottleNutrition, "totalBottleNutrition", 0f);
		Scribe_Values.Look(ref initialNutritionNeeded, "initialNutritionNeeded", 0f);
	}
}
