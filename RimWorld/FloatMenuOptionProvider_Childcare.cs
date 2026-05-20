using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Childcare : FloatMenuOptionProvider
{
	protected override bool Drafted => false;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	public override bool CanTargetDespawned => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.BiotechActive;
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!ChildcareUtility.CanSuckle(clickedPawn, out var reason))
		{
			yield break;
		}
		if (ChildcareUtility.CanBreastfeed(context.FirstSelectedPawn, out reason))
		{
			if (!ChildcareUtility.HasBreastfeedCompatibleFactions(context.FirstSelectedPawn, clickedPawn))
			{
				yield break;
			}
			if (!ChildcareUtility.CanMomAutoBreastfeedBabyNow(context.FirstSelectedPawn, clickedPawn, forced: true, out var reason2))
			{
				TaggedString taggedString = "BabyCareBreastfeedUnable".Translate(clickedPawn.Named("BABY")) + ": " + reason2.Value.Translate(context.FirstSelectedPawn, context.FirstSelectedPawn, clickedPawn).CapitalizeFirst();
				yield return new FloatMenuOption(taggedString, null);
			}
			else
			{
				Job job = ChildcareUtility.MakeBreastfeedJob(clickedPawn);
				if (context.FirstSelectedPawn.jobs.curJob != null && context.FirstSelectedPawn.jobs.curJob.JobIsSameAs(context.FirstSelectedPawn, job))
				{
					yield return new FloatMenuOption("AlreadyBreastfeeding".Translate(clickedPawn.Named("BABY")), null);
				}
				else
				{
					yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BabyCareBreastfeed".Translate(clickedPawn.Named("BABY")), delegate
					{
						context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
					}), context.FirstSelectedPawn, clickedPawn);
				}
			}
		}
		if (CaravanFormingUtility.IsFormingCaravanOrDownedPawnToBeTakenByCaravan(clickedPawn))
		{
			yield break;
		}
		if (!ChildcareUtility.CanHaulBabyNow(context.FirstSelectedPawn, clickedPawn, ignoreOtherReservations: false, out var reason3))
		{
			if (context.FirstSelectedPawn.MapHeld.reservationManager.TryGetReserver(clickedPawn, context.FirstSelectedPawn.Faction, out var reserver))
			{
				yield return new FloatMenuOption(string.Format("{0}: {1} {2}", "CannotCarryToSafePlace".Translate(), clickedPawn.LabelShort, "ReservedBy".Translate(reserver.LabelShort, reserver).Resolve().StripTags()), null);
			}
			else if (reason3.HasValue && reason3 == ChildcareUtility.BreastfeedFailReason.HaulerCannotReachBaby)
			{
				yield return new FloatMenuOption(string.Format("{0}: {1}", "CannotCarryToSafePlace".Translate(), "NoPath".Translate().CapitalizeFirst()), null);
			}
			else
			{
				yield return new FloatMenuOption(string.Format("{0}: {1}", "CannotCarryToSafePlace".Translate(), "Incapable".Translate().CapitalizeFirst()), null);
			}
			yield break;
		}
		LocalTargetInfo targetB = ChildcareUtility.SafePlaceForBaby(clickedPawn, context.FirstSelectedPawn);
		if (!targetB.IsValid)
		{
			yield break;
		}
		if (targetB.Thing is Building_Bed building_Bed)
		{
			if (clickedPawn.CurrentBed() == building_Bed)
			{
				yield break;
			}
		}
		else if (clickedPawn.Spawned && clickedPawn.Position == targetB.Cell)
		{
			yield return new FloatMenuOption(string.Format("{0}: {1}", "CannotCarryToSafePlace".Translate(), "NoBetterSafePlace".Translate().CapitalizeFirst()), null);
			yield break;
		}
		Job job2 = JobMaker.MakeJob(JobDefOf.BringBabyToSafety, clickedPawn, targetB);
		if (context.FirstSelectedPawn.jobs.curJob != null && context.FirstSelectedPawn.jobs.curJob.JobIsSameAs(context.FirstSelectedPawn, job2))
		{
			yield return new FloatMenuOption("AlreadyCarryingToSafePlace".Translate(clickedPawn.Named("BABY")), null);
			yield break;
		}
		yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("CarryToSafePlace".Translate(clickedPawn.Named("BABY")), delegate
		{
			job2.count = 1;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
		}), context.FirstSelectedPawn, clickedPawn);
	}
}
