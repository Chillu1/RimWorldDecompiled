using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Breastfeed : JobDriver_FeedBaby
{
	protected override Toil FeedingToil { get; set; }

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (base.TryMakePreToilReservations(errorOnFailed))
		{
			return pawn.Reserve(pawn, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	public override void SetInitialPosture()
	{
	}

	public override bool CanBeginNowWhileLyingDown()
	{
		return pawn.Downed;
	}

	protected override IEnumerable<Toil> FeedBaby()
	{
		AddFailCondition(() => !ChildcareUtility.CanBreastfeed(pawn, out var _));
		yield return Breastfeed();
		yield return TuckMomInIfDowned();
	}

	private Toil Breastfeed()
	{
		FeedingToil = ToilMaker.MakeToil("Breastfeed");
		FeedingToil.initAction = delegate
		{
			base.Baby.jobs.StartJob(ChildcareUtility.MakeBabySuckleJob(pawn), JobCondition.InterruptForced);
			initialFoodPercentage = base.Baby.needs.food.CurLevelPercentage;
		};
		FeedingToil.tickIntervalAction = delegate(int delta)
		{
			bool num = ChildcareUtility.SuckleFromLactatingPawn(base.Baby, pawn, delta);
			pawn.GainComfortFromCellIfPossible(delta);
			if (!pawn.Downed && pawn.Rotation == Rot4.North)
			{
				pawn.Rotation = Rot4.East;
			}
			if (!num)
			{
				ReadyForNextToil();
			}
		};
		FeedingToil.AddFinishAction(delegate
		{
			if (!base.Baby.Dead)
			{
				if (base.Baby.needs.food.CurLevelPercentage - initialFoodPercentage > 0.6f)
				{
					base.Baby.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BreastfedMe, pawn);
					pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BreastfedBaby, base.Baby);
				}
				if (base.Baby.CurJobDef == JobDefOf.BabySuckle)
				{
					base.Baby.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
				if (pawn.Downed)
				{
					pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
				}
			}
		});
		FeedingToil.AddFailCondition(() => !ChildcareUtility.CanBreastfeed(pawn, out var _));
		FeedingToil.handlingFacing = true;
		FeedingToil.WithProgressBar(TargetIndex.A, () => base.Baby.needs.food.CurLevelPercentage);
		FeedingToil.defaultCompleteMode = ToilCompleteMode.Never;
		FeedingToil.WithEffect(EffecterDefOf.Breastfeeding, TargetIndex.A);
		return FeedingToil;
	}

	private Toil TuckMomInIfDowned()
	{
		Toil toil = ToilMaker.MakeToil("TuckMomInIfDowned");
		toil.initAction = delegate
		{
			if (HealthAIUtility.ShouldSeekMedicalRest(pawn))
			{
				Building_Bed building_Bed = pawn.CurrentBed();
				if (building_Bed != null && building_Bed == RestUtility.FindPatientBedFor(pawn))
				{
					pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.LayDown, building_Bed), JobCondition.Succeeded, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, JobTag.RestingForMedicalReasons);
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}
}
