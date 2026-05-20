using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_CarryToBiosculpterPod : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.IdeologyActive;
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if ((!clickedPawn.IsColonistPlayerControlled && !clickedPawn.IsPrisonerOfColony) || (!clickedPawn.IsPrisoner && !clickedPawn.Downed) || (clickedPawn.IsPrisoner && PrisonBreakUtility.IsPrisonBreaking(clickedPawn)) || !context.FirstSelectedPawn.CanReserveAndReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
		{
			yield break;
		}
		Thing thing = CompBiosculpterPod.FindPodFor(context.FirstSelectedPawn, clickedPawn, biotuned: true) ?? CompBiosculpterPod.FindPodFor(context.FirstSelectedPawn, clickedPawn, biotuned: false);
		if (thing == null || !thing.TryGetComp(out CompBiosculpterPod podComp))
		{
			yield break;
		}
		foreach (CompBiosculpterPod_Cycle cycle in podComp.AvailableCycles)
		{
			TaggedString taggedString = "CarryToBiosculpterPod".Translate(clickedPawn.Named("PAWN"), cycle.Props.label.Named("CYCLE"));
			if (clickedPawn.IsQuestLodger())
			{
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString + " (" + "CryptosleepCasketGuestsNotAllowed".Translate() + ")", null, MenuOptionPriority.Default, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
				yield break;
			}
			if (clickedPawn.GetExtraHostFaction() != null)
			{
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString + " (" + "CryptosleepCasketGuestPrisonersNotAllowed".Translate() + ")", null, MenuOptionPriority.Default, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
				yield break;
			}
			string text = podComp.CannotUseNowCycleReason(cycle) ?? podComp.CannotUseNowPawnCycleReason(context.FirstSelectedPawn, clickedPawn, cycle);
			if (text != null)
			{
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(CompBiosculpterPod.CannotStartText(cycle, text), null, MenuOptionPriority.Default, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
				yield break;
			}
			Action action = delegate
			{
				if (!podComp.CanAcceptOnceCycleChosen(clickedPawn))
				{
					Messages.Message("CannotCarryToBiosculpterPod".Translate() + ": " + "NoBiosculpterPod".Translate(), clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					podComp.PrepareCycleJob(context.FirstSelectedPawn, clickedPawn, cycle, podComp.MakeCarryToBiosculpterJob(clickedPawn));
				}
			};
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, action, MenuOptionPriority.Default, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
		}
	}
}
