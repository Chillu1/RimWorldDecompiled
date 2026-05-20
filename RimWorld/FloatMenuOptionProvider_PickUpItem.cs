using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_PickUpItem : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return !context.FirstSelectedPawn.IsFormingCaravan();
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		if (clickedThing.def.category != ThingCategory.Item || !clickedThing.def.EverHaulable || !PawnUtility.CanPickUp(context.FirstSelectedPawn, clickedThing.def) || (context.FirstSelectedPawn.Map.IsPlayerHome && !JobGiver_DropUnusedInventory.ShouldKeepDrugInInventory(context.FirstSelectedPawn, clickedThing)))
		{
			yield break;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			yield return new FloatMenuOption("CannotPickUp".Translate(clickedThing.Label, clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			yield break;
		}
		if (MassUtility.WillBeOverEncumberedAfterPickingUp(context.FirstSelectedPawn, clickedThing, 1))
		{
			yield return new FloatMenuOption("CannotPickUp".Translate(clickedThing.Label, clickedThing) + ": " + "TooHeavy".Translate(), null);
			yield break;
		}
		int maxAllowedToPickUp = PawnUtility.GetMaxAllowedToPickUp(context.FirstSelectedPawn, clickedThing.def);
		if (maxAllowedToPickUp == 0)
		{
			yield return new FloatMenuOption("CannotPickUp".Translate(clickedThing.Label, clickedThing) + ": " + "MaxPickUpAllowed".Translate(clickedThing.def.orderedTakeGroup.max, clickedThing.def.orderedTakeGroup.label), null);
			yield break;
		}
		if (clickedThing.stackCount == 1 || maxAllowedToPickUp == 1)
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpOne".Translate(clickedThing.LabelNoCount, clickedThing), delegate
			{
				clickedThing.SetForbidden(value: false, warnOnFail: false);
				Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, clickedThing);
				job.count = 1;
				job.checkEncumbrance = true;
				job.takeInventoryDelay = 120;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
			yield break;
		}
		if (maxAllowedToPickUp < clickedThing.stackCount)
		{
			yield return new FloatMenuOption("CannotPickUpAll".Translate(clickedThing.Label, clickedThing) + ": " + "MaxPickUpAllowed".Translate(clickedThing.def.orderedTakeGroup.max, clickedThing.def.orderedTakeGroup.label), null);
		}
		else if (MassUtility.WillBeOverEncumberedAfterPickingUp(context.FirstSelectedPawn, clickedThing, clickedThing.stackCount))
		{
			yield return new FloatMenuOption("CannotPickUpAll".Translate(clickedThing.Label, clickedThing) + ": " + "TooHeavy".Translate(), null);
		}
		else
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpAll".Translate(clickedThing.Label, clickedThing), delegate
			{
				clickedThing.SetForbidden(value: false, warnOnFail: false);
				Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, clickedThing);
				job.count = clickedThing.stackCount;
				job.checkEncumbrance = true;
				job.takeInventoryDelay = 120;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
		}
		yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpSome".Translate(clickedThing.LabelNoCount, clickedThing), delegate
		{
			int b = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(context.FirstSelectedPawn, clickedThing), clickedThing.stackCount);
			int to = Mathf.Min(maxAllowedToPickUp, b);
			Dialog_Slider window = new Dialog_Slider("PickUpCount".Translate(clickedThing.LabelNoCount, clickedThing), 1, to, delegate(int count)
			{
				clickedThing.SetForbidden(value: false, warnOnFail: false);
				Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, clickedThing);
				job.count = count;
				job.checkEncumbrance = true;
				job.takeInventoryDelay = 120;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			});
			Find.WindowStack.Add(window);
		}, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
	}
}
