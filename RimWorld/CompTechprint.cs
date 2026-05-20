using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompTechprint : ThingComp
{
	public CompProperties_Techprint Props => (CompProperties_Techprint)props;

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (!ModLister.CheckRoyalty("Techprint"))
		{
			yield break;
		}
		JobFailReason.Clear();
		if (selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Research) || selPawn.WorkTagIsDisabled(WorkTags.Intellectual))
		{
			JobFailReason.Is("WillNever".Translate("Research".TranslateSimple().UncapitalizeFirst()));
		}
		else if (!selPawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Some))
		{
			JobFailReason.Is("CannotReach".Translate());
		}
		HaulAIUtility.PawnCanAutomaticallyHaul(selPawn, parent, forced: true);
		Thing thing = GenClosest.ClosestThingReachable(selPawn.Position, selPawn.Map, ThingRequest.ForGroup(ThingRequestGroup.ResearchBench), PathEndMode.InteractionCell, TraverseParms.For(selPawn, Danger.Some), 9999f, (Thing thing2) => thing2 is Building_ResearchBench && !thing2.IsForbidden(selPawn) && selPawn.CanReserve(thing2));
		Job job = null;
		if (thing != null)
		{
			job = JobMaker.MakeJob(JobDefOf.ApplyTechprint);
			job.targetA = thing;
			job.targetB = parent;
			job.targetC = thing.Position;
		}
		if (JobFailReason.HaveReason)
		{
			yield return new FloatMenuOption("CannotGenericWorkCustom".Translate("ApplyTechprint".Translate(parent.Label)) + ": " + JobFailReason.Reason.CapitalizeFirst(), null);
			JobFailReason.Clear();
			yield break;
		}
		yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ApplyTechprint".Translate(parent.Label).CapitalizeFirst(), delegate
		{
			if (job == null)
			{
				Messages.Message("MessageNoResearchBenchForTechprint".Translate(), MessageTypeDefOf.RejectInput);
			}
			else
			{
				selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}), selPawn, parent);
	}
}
