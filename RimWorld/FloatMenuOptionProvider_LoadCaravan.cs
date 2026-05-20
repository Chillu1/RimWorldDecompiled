using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class FloatMenuOptionProvider_LoadCaravan : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return context.FirstSelectedPawn.IsFormingCaravan();
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		if (clickedThing.def.category != ThingCategory.Item || !clickedThing.def.EverHaulable || !clickedThing.def.canLoadIntoCaravan)
		{
			yield break;
		}
		Pawn packTarget = GiveToPackAnimalUtility.UsablePackAnimalWithTheMostFreeSpace(context.FirstSelectedPawn) ?? context.FirstSelectedPawn;
		JobDef jobDef = ((packTarget == context.FirstSelectedPawn) ? JobDefOf.TakeInventory : JobDefOf.GiveToPackAnimal);
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			yield return new FloatMenuOption("CannotLoadIntoCaravan".Translate(clickedThing.Label, clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			yield break;
		}
		if (MassUtility.WillBeOverEncumberedAfterPickingUp(packTarget, clickedThing, 1))
		{
			yield return new FloatMenuOption("CannotLoadIntoCaravan".Translate(clickedThing.Label, clickedThing) + ": " + "TooHeavy".Translate(), null);
			yield break;
		}
		LordJob_FormAndSendCaravan lordJob = (LordJob_FormAndSendCaravan)context.FirstSelectedPawn.GetLord().LordJob;
		float capacityLeft = CaravanFormingUtility.CapacityLeft(lordJob);
		if (clickedThing.stackCount == 1)
		{
			float capacityLeft2 = capacityLeft - clickedThing.GetStatValue(StatDefOf.Mass);
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(CaravanFormingUtility.AppendOverweightInfo("LoadIntoCaravan".Translate(clickedThing.Label, clickedThing), capacityLeft2), delegate
			{
				clickedThing.SetForbidden(value: false, warnOnFail: false);
				Job job = JobMaker.MakeJob(jobDef, clickedThing);
				job.count = 1;
				job.checkEncumbrance = packTarget == context.FirstSelectedPawn;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
			yield break;
		}
		if (MassUtility.WillBeOverEncumberedAfterPickingUp(packTarget, clickedThing, clickedThing.stackCount))
		{
			yield return new FloatMenuOption("CannotLoadIntoCaravanAll".Translate(clickedThing.Label, clickedThing) + ": " + "TooHeavy".Translate(), null);
		}
		else
		{
			float capacityLeft3 = capacityLeft - (float)clickedThing.stackCount * clickedThing.GetStatValue(StatDefOf.Mass);
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(CaravanFormingUtility.AppendOverweightInfo("LoadIntoCaravanAll".Translate(clickedThing.Label, clickedThing), capacityLeft3), delegate
			{
				clickedThing.SetForbidden(value: false, warnOnFail: false);
				Job job = JobMaker.MakeJob(jobDef, clickedThing);
				job.count = clickedThing.stackCount;
				job.checkEncumbrance = packTarget == context.FirstSelectedPawn;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
		}
		yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("LoadIntoCaravanSome".Translate(clickedThing.LabelNoCount, clickedThing), delegate
		{
			int to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(packTarget, clickedThing), clickedThing.stackCount);
			Dialog_Slider window = new Dialog_Slider(delegate(int val)
			{
				float capacityLeft4 = capacityLeft - (float)val * clickedThing.GetStatValue(StatDefOf.Mass);
				return CaravanFormingUtility.AppendOverweightInfo("LoadIntoCaravanCount".Translate(clickedThing.LabelNoCount, clickedThing).Formatted(val), capacityLeft4);
			}, 1, to, delegate(int count)
			{
				clickedThing.SetForbidden(value: false, warnOnFail: false);
				Job job = JobMaker.MakeJob(jobDef, clickedThing);
				job.count = count;
				job.checkEncumbrance = packTarget == context.FirstSelectedPawn;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			});
			Find.WindowStack.Add(window);
		}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
	}
}
