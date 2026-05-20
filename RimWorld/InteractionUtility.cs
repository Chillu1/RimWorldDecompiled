using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class InteractionUtility
{
	public static void OrderInteraction(Pawn pawn, Thing thing, ThingDef optionalItem = null, int optionItemCount = 1)
	{
		Job job;
		if (optionalItem != null)
		{
			List<Thing> list = HaulAIUtility.FindFixedIngredientCount(pawn, optionalItem, optionItemCount);
			if (list.NullOrEmpty())
			{
				return;
			}
			job = JobMaker.MakeJob(JobDefOf.InteractThing, thing, list[0]);
			job.targetQueueB = (from i in list.Skip(1)
				select new LocalTargetInfo(i)).ToList();
		}
		else
		{
			job = JobMaker.MakeJob(JobDefOf.InteractThing, thing);
		}
		job.count = optionItemCount;
		job.playerForced = true;
		pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}

	public static void OrderInteraction(Pawn pawn, ThingComp comp, ThingDef optionalItem = null, int optionItemCount = 1)
	{
		if (comp.Isnt<CompInteractable>())
		{
			Log.Error("Cannot order an interaction from a comp (" + comp.GetType().Name + ") which does not inherit CompInteractable");
			return;
		}
		Job job;
		if (optionalItem != null)
		{
			List<Thing> list = HaulAIUtility.FindFixedIngredientCount(pawn, optionalItem, optionItemCount);
			if (list.NullOrEmpty())
			{
				Log.Error("Failed to find required fixed ingredients for interaction");
				return;
			}
			job = JobMaker.MakeJob(JobDefOf.InteractThing, comp.parent, list[0]);
			job.targetQueueB = (from i in list.Skip(1)
				select new LocalTargetInfo(i)).ToList();
		}
		else
		{
			job = JobMaker.MakeJob(JobDefOf.InteractThing, comp.parent);
		}
		job.count = optionItemCount;
		job.playerForced = true;
		job.interactableIndex = comp.parent.AllComps.IndexOf(comp);
		pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}
}
