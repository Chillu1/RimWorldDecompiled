using System;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Wear : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return context.FirstSelectedPawn.apparel != null;
	}

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		Apparel apparel = clickedThing as Apparel;
		if (apparel == null)
		{
			return null;
		}
		string key = "CannotWear";
		string key2 = "ForceWear";
		if (apparel.def.apparel.LastLayer.IsUtilityLayer)
		{
			key = "CannotEquipApparel";
			key2 = "ForceEquipApparel";
		}
		if (!context.FirstSelectedPawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		if (apparel.IsBurning())
		{
			return new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + "Burning".Translate(), null);
		}
		if (context.FirstSelectedPawn.apparel.WouldReplaceLockedApparel(apparel))
		{
			return new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + "WouldReplaceLockedApparel".Translate().CapitalizeFirst(), null);
		}
		if (context.FirstSelectedPawn.IsMutant && context.FirstSelectedPawn.mutant.Def.disableApparel)
		{
			return new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + context.FirstSelectedPawn.mutant.Def.LabelCap, null);
		}
		if (!ApparelUtility.HasPartsToWear(context.FirstSelectedPawn, apparel.def))
		{
			return new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + "CannotWearBecauseOfMissingBodyParts".Translate().CapitalizeFirst(), null);
		}
		if (!EquipmentUtility.CanEquip(apparel, context.FirstSelectedPawn, out var cantReason))
		{
			return new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + cantReason, null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(key2.Translate(apparel.LabelShort, apparel), delegate
		{
			Action action = delegate
			{
				apparel.SetForbidden(value: false);
				Job job = JobMaker.MakeJob(JobDefOf.Wear, apparel);
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			};
			Apparel apparelReplacedByNewApparel = ApparelUtility.GetApparelReplacedByNewApparel(context.FirstSelectedPawn, apparel);
			if (apparelReplacedByNewApparel == null || !ModsConfig.BiotechActive || !MechanitorUtility.TryConfirmBandwidthLossFromDroppingThing(context.FirstSelectedPawn, apparelReplacedByNewApparel, action))
			{
				action();
			}
		}, MenuOptionPriority.High), context.FirstSelectedPawn, apparel);
	}
}
