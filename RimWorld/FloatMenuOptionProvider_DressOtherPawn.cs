using System;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_DressOtherPawn : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		Apparel apparel = clickedThing as Apparel;
		if (apparel == null)
		{
			return null;
		}
		string cannotForceTargetText = "CannotForceTargetToWear";
		string key = "ForceTargetToWear";
		if (apparel.def.apparel.LastLayer.IsUtilityLayer)
		{
			cannotForceTargetText = "CannotForceTargetToEquipApparel";
			key = "ForceTargetToEquipApparel";
		}
		if (!context.FirstSelectedPawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return new FloatMenuOption(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		if (apparel.IsBurning())
		{
			return new FloatMenuOption(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + "Burning".Translate(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(key.Translate(apparel.LabelShort, apparel), delegate
		{
			bool queueOrder = KeyBindingDefOf.QueueOrder.IsDownEvent;
			Find.Targeter.BeginTargeting(TargetingParameters.ForForceWear(context.FirstSelectedPawn), delegate(LocalTargetInfo target)
			{
				string cantReason;
				if (!target.TryGetPawn(out var targetPawn))
				{
					if (ModsConfig.OdysseyActive && target.Thing is Building_OutfitStand building_OutfitStand)
					{
						if (!building_OutfitStand.CanEverStoreThing(apparel))
						{
							Messages.Message("CannotStoreThingOnTarget".Translate(apparel.Named("THING"), building_OutfitStand.Named("TARGET")), MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.PutApparelOnOutfitStand, apparel, building_OutfitStand), requestQueueing: queueOrder, tag: JobTag.Misc);
						}
					}
				}
				else if (targetPawn.apparel.WouldReplaceLockedApparel(apparel))
				{
					Messages.Message(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + "WouldReplaceLockedApparel".Translate().CapitalizeFirst(), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				else if (targetPawn.IsMutant && targetPawn.mutant.Def.disableApparel)
				{
					Messages.Message(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + targetPawn.mutant.Def.LabelCap, targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				else if (!ApparelUtility.HasPartsToWear(targetPawn, apparel.def))
				{
					Messages.Message(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + "CannotWearBecauseOfMissingBodyParts".Translate().CapitalizeFirst(), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				else if (!EquipmentUtility.CanEquip(apparel, targetPawn, out cantReason))
				{
					Messages.Message(cannotForceTargetText.Translate(apparel.Label, apparel) + ": " + cantReason, targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					Action action = delegate
					{
						apparel.SetForbidden(value: false);
						Job job = JobMaker.MakeJob(JobDefOf.ForceTargetWear, targetPawn, apparel);
						context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, requestQueueing: queueOrder, tag: JobTag.Misc);
					};
					Apparel apparelReplacedByNewApparel = ApparelUtility.GetApparelReplacedByNewApparel(targetPawn, apparel);
					if (apparelReplacedByNewApparel == null || !ModsConfig.BiotechActive || !MechanitorUtility.TryConfirmBandwidthLossFromDroppingThing(targetPawn, apparelReplacedByNewApparel, action))
					{
						action();
					}
				}
			});
		}), context.FirstSelectedPawn, apparel);
	}
}
