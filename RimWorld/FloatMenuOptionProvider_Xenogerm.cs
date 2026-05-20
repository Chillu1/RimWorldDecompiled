using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Xenogerm : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!GeneUtility.CanAbsorbXenogerm(context.FirstSelectedPawn))
		{
			return false;
		}
		return true;
	}

	protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!context.FirstSelectedPawn.CanReserveAndReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
		{
			return new FloatMenuOption("CannotAbsorbXenogerm".Translate(clickedPawn.Named("PAWN")) + ": " + "NoPath".Translate(), null);
		}
		if (context.FirstSelectedPawn.IsQuestLodger())
		{
			return new FloatMenuOption("CannotAbsorbXenogerm".Translate(clickedPawn.Named("PAWN")) + ": " + "TemporaryFactionMember".Translate(context.FirstSelectedPawn.Named("PAWN")), null);
		}
		if (GeneUtility.SameXenotype(context.FirstSelectedPawn, clickedPawn))
		{
			return new FloatMenuOption("CannotAbsorbXenogerm".Translate(clickedPawn.Named("PAWN")) + ": " + "SameXenotype".Translate(context.FirstSelectedPawn.Named("PAWN")), null);
		}
		if (clickedPawn.health.hediffSet.HasHediff(HediffDefOf.XenogermLossShock))
		{
			return new FloatMenuOption("CannotAbsorbXenogerm".Translate(clickedPawn.Named("PAWN")) + ": " + "XenogermLossShockPresent".Translate(clickedPawn.Named("PAWN")), null);
		}
		if (!CompAbilityEffect_ReimplantXenogerm.PawnIdeoCanAcceptReimplant(clickedPawn, context.FirstSelectedPawn))
		{
			return new FloatMenuOption("CannotAbsorbXenogerm".Translate(clickedPawn.Named("PAWN")) + ": " + "IdeoligionForbids".Translate(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("AbsorbXenogerm".Translate(clickedPawn.Named("PAWN")), delegate
		{
			if (clickedPawn.IsPrisonerOfColony && !clickedPawn.Downed)
			{
				Messages.Message("MessageTargetMustBeDownedToForceReimplant".Translate(clickedPawn.Named("PAWN")), clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			else if (GeneUtility.PawnWouldDieFromReimplanting(clickedPawn))
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("WarningPawnWillDieFromReimplanting".Translate(clickedPawn.Named("PAWN")), delegate
				{
					GeneUtility.GiveReimplantJob(context.FirstSelectedPawn, clickedPawn);
				}, destructive: true));
			}
			else
			{
				GeneUtility.GiveReimplantJob(context.FirstSelectedPawn, clickedPawn);
			}
		}), context.FirstSelectedPawn, clickedPawn);
	}
}
