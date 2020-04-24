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
			JobFailReason.Clear();
			if (selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Research) || selPawn.WorkTagIsDisabled(WorkTags.Intellectual))
			{
				JobFailReason.Is("WillNever".Translate("Research".TranslateSimple().UncapitalizeFirst()));
			}
			else if (!selPawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Some))
			{
				JobFailReason.Is("CannotReach".Translate());
			}
			else if (!selPawn.CanReserve(parent))
			{
				Pawn pawn = selPawn.Map.reservationManager.FirstRespectedReserver(parent, selPawn);
				if (pawn == null)
				{
					pawn = selPawn.Map.physicalInteractionReservationManager.FirstReserverOf(selPawn);
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
			HaulAIUtility.PawnCanAutomaticallyHaul(selPawn, parent, forced: true);
			Thing thing2 = GenClosest.ClosestThingReachable(selPawn.Position, selPawn.Map, ThingRequest.ForGroup(ThingRequestGroup.ResearchBench), PathEndMode.InteractionCell, TraverseParms.For(selPawn, Danger.Some), 9999f, (Thing thing) => thing is Building_ResearchBench && selPawn.CanReserve(thing));
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
			}
			else
			{
				yield return new FloatMenuOption("ApplyTechprint".Translate(parent.Label).CapitalizeFirst(), delegate
				{
					if (job == null)
					{
						Messages.Message("MessageNoResearchBenchForTechprint".Translate(), MessageTypeDefOf.RejectInput);
					}
					else
					{
						selPawn.jobs.TryTakeOrderedJob(job);
					}
				});
			}
		}
	}
}
