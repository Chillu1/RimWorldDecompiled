using RimWorld;

namespace Verse;

public class HediffComp_GiveHediff : HediffComp
{
	private HediffCompProperties_GiveHediff Props => (HediffCompProperties_GiveHediff)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (!(parent.Severity < Props.atSeverity) && (!Props.skipIfAlreadyExists || !base.Pawn.health.hediffSet.HasHediff(Props.hediffDef)))
		{
			base.Pawn.health.AddHediff(Props.hediffDef);
			if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
			{
				SendLetter();
			}
			if (Props.disappearsAfterGiving)
			{
				base.Pawn.health.RemoveHediff(parent);
			}
		}
	}

	private void SendLetter()
	{
		if (Props.letterLabel != null)
		{
			Find.LetterStack.ReceiveLetter(Props.letterLabel.Formatted(base.Pawn.Named("PAWN")), Props.letterText.Formatted(base.Pawn.Named("PAWN")), Props.letterDef ?? LetterDefOf.NegativeEvent, base.Pawn);
		}
	}
}
