using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_HandleCorpse : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => false;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		Corpse corpse = clickedThing as Corpse;
		if (corpse == null)
		{
			yield break;
		}
		Building_GibbetCage cage = Building_GibbetCage.FindGibbetCageFor(corpse, context.FirstSelectedPawn);
		if (cage != null)
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PlaceIn".Translate(corpse, cage), delegate
			{
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(HaulAIUtility.HaulToContainerJob(context.FirstSelectedPawn, corpse, cage), JobTag.Misc);
			}), context.FirstSelectedPawn, new LocalTargetInfo(corpse));
		}
		if (!corpse.IsInValidStorage())
		{
			yield break;
		}
		StoragePriority priority = StoreUtility.CurrentHaulDestinationOf(corpse).GetStoreSettings().Priority;
		if (!StoreUtility.TryFindBestBetterNonSlotGroupStorageFor(corpse, context.FirstSelectedPawn, context.FirstSelectedPawn.Map, priority, Faction.OfPlayer, out var haulDestination, acceptSamePriority: true) || haulDestination.GetStoreSettings().Priority != priority)
		{
			yield break;
		}
		Building_Grave grave = haulDestination as Building_Grave;
		if (grave != null)
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PrioritizeGeneric".Translate("Burying".Translate(), corpse.Label).CapitalizeFirst(), delegate
			{
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(HaulAIUtility.HaulToContainerJob(context.FirstSelectedPawn, corpse, grave), JobTag.Misc);
			}), context.FirstSelectedPawn, new LocalTargetInfo(corpse));
		}
	}
}
