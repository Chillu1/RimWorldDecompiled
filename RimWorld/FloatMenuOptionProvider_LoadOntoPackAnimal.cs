using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_LoadOntoPackAnimal : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (!context.FirstSelectedPawn.IsFormingCaravan())
		{
			return !context.map.IsPlayerHome;
		}
		return false;
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		if (clickedThing.def.category != ThingCategory.Item || !clickedThing.def.EverHaulable)
		{
			yield break;
		}
		Pawn bestPackAnimal = GiveToPackAnimalUtility.UsablePackAnimalWithTheMostFreeSpace(context.FirstSelectedPawn);
		if (bestPackAnimal == null)
		{
			yield break;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			yield return new FloatMenuOption("CannotGiveToPackAnimal".Translate(clickedThing.Label, clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			yield break;
		}
		if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, clickedThing, 1))
		{
			yield return new FloatMenuOption("CannotGiveToPackAnimal".Translate(clickedThing.Label, clickedThing) + ": " + "TooHeavy".Translate(), null);
			yield break;
		}
		if (clickedThing.stackCount == 1)
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimal".Translate(clickedThing.Label, clickedThing), delegate
			{
				clickedThing.SetForbidden(value: false, warnOnFail: false);
				Job job = JobMaker.MakeJob(JobDefOf.GiveToPackAnimal, clickedThing);
				job.count = 1;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
			yield break;
		}
		if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, clickedThing, clickedThing.stackCount))
		{
			yield return new FloatMenuOption("CannotGiveToPackAnimalAll".Translate(clickedThing.Label, clickedThing) + ": " + "TooHeavy".Translate(), null);
		}
		else
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimalAll".Translate(clickedThing.Label, clickedThing), delegate
			{
				clickedThing.SetForbidden(value: false, warnOnFail: false);
				Job job = JobMaker.MakeJob(JobDefOf.GiveToPackAnimal, clickedThing);
				job.count = clickedThing.stackCount;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
		}
		yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimalSome".Translate(clickedThing.LabelNoCount, clickedThing), delegate
		{
			int to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(bestPackAnimal, clickedThing), clickedThing.stackCount);
			Dialog_Slider window = new Dialog_Slider("GiveToPackAnimalCount".Translate(clickedThing.LabelNoCount, clickedThing), 1, to, delegate(int count)
			{
				clickedThing.SetForbidden(value: false, warnOnFail: false);
				Job job = JobMaker.MakeJob(JobDefOf.GiveToPackAnimal, clickedThing);
				job.count = count;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			});
			Find.WindowStack.Add(window);
		}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
	}
}
