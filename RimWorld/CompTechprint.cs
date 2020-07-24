using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompTechprint : ThingComp
	{
		public CompProperties_Techprint Props => (CompProperties_Techprint)props;

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			Pawn selPawn2 = selPawn;
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Techprints are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it.", 657212);
				yield break;
			}
			JobFailReason.Clear();
			if (selPawn2.WorkTypeIsDisabled(WorkTypeDefOf.Research) || selPawn2.WorkTagIsDisabled(WorkTags.Intellectual))
			{
				JobFailReason.Is("WillNever".Translate("Research".TranslateSimple().UncapitalizeFirst()));
			}
			else if (!selPawn2.CanReach(parent, PathEndMode.ClosestTouch, Danger.Some))
			{
				JobFailReason.Is("CannotReach".Translate());
			}
			else if (!selPawn2.CanReserve(parent))
			{
				Pawn pawn = selPawn2.Map.reservationManager.FirstRespectedReserver(parent, selPawn2);
				if (pawn == null)
				{
					pawn = selPawn2.Map.physicalInteractionReservationManager.FirstReserverOf(selPawn2);
				}
				if (pawn != null)
				{
					JobFailReason.Is("ReservedBy".Translate(pawn.LabelShort, pawn));
				}
				else
				{
					JobFailReason.Is("Reserved".Translate());
				}
			}
			HaulAIUtility.PawnCanAutomaticallyHaul(selPawn2, parent, forced: true);
			Thing thing2 = GenClosest.ClosestThingReachable(selPawn2.Position, selPawn2.Map, ThingRequest.ForGroup(ThingRequestGroup.ResearchBench), PathEndMode.InteractionCell, TraverseParms.For(selPawn2, Danger.Some), 9999f, (Thing thing) => thing is Building_ResearchBench && selPawn2.CanReserve(thing));
			Job job = null;
			if (thing2 != null)
			{
				job = JobMaker.MakeJob(JobDefOf.ApplyTechprint);
				job.targetA = thing2;
				job.targetB = parent;
				job.targetC = thing2.Position;
			}
			if (JobFailReason.HaveReason)
			{
				yield return new FloatMenuOption("CannotGenericWorkCustom".Translate("ApplyTechprint".Translate(parent.Label)) + ": " + JobFailReason.Reason.CapitalizeFirst(), null);
				JobFailReason.Clear();
				yield break;
			}
			yield return new FloatMenuOption("ApplyTechprint".Translate(parent.Label).CapitalizeFirst(), delegate
			{
				if (job == null)
				{
					Messages.Message("MessageNoResearchBenchForTechprint".Translate(), MessageTypeDefOf.RejectInput);
				}
				else
				{
					selPawn2.jobs.TryTakeOrderedJob(job);
				}
			});
		}
	}
}
