using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_EnterMapPortal : FloatMenuOptionProvider
{
	private static List<Pawn> tmpPortalEnteringPawns = new List<Pawn>();

	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => true;

	protected override bool MechanoidCanDo => true;

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		MapPortal portal = clickedThing as MapPortal;
		if (portal == null)
		{
			return null;
		}
		if (!portal.IsEnterable(out var reason))
		{
			return new FloatMenuOption("CannotEnterPortal".Translate(portal.Label) + ": " + reason, null);
		}
		if (!context.IsMultiselect)
		{
			AcceptanceReport acceptanceReport = CanEnterPortal(context.FirstSelectedPawn, portal);
			if (!acceptanceReport.Accepted)
			{
				return new FloatMenuOption("CannotEnterPortal".Translate(portal.Label) + ": " + acceptanceReport.Reason, null);
			}
		}
		tmpPortalEnteringPawns.Clear();
		foreach (Pawn validSelectedPawn in context.ValidSelectedPawns)
		{
			if ((bool)CanEnterPortal(context.FirstSelectedPawn, portal))
			{
				tmpPortalEnteringPawns.Add(validSelectedPawn);
			}
		}
		if (tmpPortalEnteringPawns.NullOrEmpty())
		{
			return null;
		}
		return new FloatMenuOption(portal.EnterString, delegate
		{
			foreach (Pawn tmpPortalEnteringPawn in tmpPortalEnteringPawns)
			{
				Job job = JobMaker.MakeJob(JobDefOf.EnterPortal, portal);
				job.playerForced = true;
				tmpPortalEnteringPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}, MenuOptionPriority.High);
	}

	private static AcceptanceReport CanEnterPortal(Pawn pawn, MapPortal portal)
	{
		if (!pawn.CanReach(portal, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return "NoPath".Translate();
		}
		if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return "Incapable".Translate();
		}
		return true;
	}
}
