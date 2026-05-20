using System;
using System.Collections.Generic;
using RimWorld.Utility;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Reload : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		foreach (IReloadableComp reloadable in GetReloadablesUsingAmmo(context.FirstSelectedPawn, clickedThing))
		{
			ThingComp thingComp = reloadable as ThingComp;
			string text = "Reload".Translate(thingComp.parent.Named("GEAR"), NamedArgumentUtility.Named(reloadable.AmmoDef, "AMMO")) + " (" + reloadable.LabelRemaining + ")";
			if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
			{
				yield return new FloatMenuOption(text + ": " + "NoPath".Translate().CapitalizeFirst(), null);
				continue;
			}
			if (!reloadable.NeedsReload(allowForceReload: true))
			{
				yield return new FloatMenuOption(text + ": " + "ReloadFull".Translate(), null);
				continue;
			}
			List<Thing> chosenAmmo;
			if ((chosenAmmo = ReloadableUtility.FindEnoughAmmo(context.FirstSelectedPawn, clickedThing.Position, reloadable, forceReload: true)) == null)
			{
				yield return new FloatMenuOption(text + ": " + "ReloadNotEnough".Translate(), null);
				continue;
			}
			if (context.FirstSelectedPawn.carryTracker.AvailableStackSpace(reloadable.AmmoDef) < reloadable.MinAmmoNeeded(allowForcedReload: true))
			{
				yield return new FloatMenuOption(text + ": " + "ReloadCannotCarryEnough".Translate(NamedArgumentUtility.Named(reloadable.AmmoDef, "AMMO")), null);
				continue;
			}
			Action action = delegate
			{
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobGiver_Reload.MakeReloadJob(reloadable, chosenAmmo), JobTag.Misc);
			};
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, action), context.FirstSelectedPawn, clickedThing);
		}
	}

	private IEnumerable<IReloadableComp> GetReloadablesUsingAmmo(Pawn pawn, Thing clickedThing)
	{
		if (pawn.equipment?.PrimaryEq != null && pawn.equipment.PrimaryEq is IReloadableComp reloadableComp && clickedThing.def == reloadableComp.AmmoDef)
		{
			yield return reloadableComp;
		}
		if (pawn.apparel == null)
		{
			yield break;
		}
		foreach (Apparel item in pawn.apparel.WornApparel)
		{
			IReloadableComp reloadableComp2 = item.TryGetComp<CompApparelReloadable>();
			if (reloadableComp2 != null && clickedThing.def == reloadableComp2.AmmoDef)
			{
				yield return reloadableComp2;
			}
		}
	}
}
