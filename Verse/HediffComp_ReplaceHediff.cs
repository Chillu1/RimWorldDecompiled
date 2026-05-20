using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public class HediffComp_ReplaceHediff : HediffComp
{
	private static readonly List<Hediff> added = new List<Hediff>();

	public HediffCompProperties_ReplaceHediff Props => (HediffCompProperties_ReplaceHediff)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (!Props.manuallyTriggered && !(parent.Severity < Props.severity))
		{
			Trigger();
		}
	}

	public void Trigger()
	{
		if (!Props.message.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(parent.pawn))
		{
			Messages.Message(Props.message.Formatted(parent.pawn.Named("PAWN")), parent.pawn, Props.messageDef ?? MessageTypeDefOf.NegativeEvent);
		}
		added.Clear();
		foreach (HediffCompProperties_ReplaceHediff.TriggeredHediff hediff in Props.hediffs)
		{
			hediff.ApplyTo(parent.pawn, added);
		}
		if (added.Empty())
		{
			parent.pawn.health.RemoveHediff(parent);
			return;
		}
		if (!Props.letterLabel.NullOrEmpty() && !Props.letterDesc.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(parent.pawn))
		{
			string arg = (from x in added
				where x.Part != null
				select x.Part.LabelCap).ToLineList("  - ");
			TaggedString label = Props.letterLabel.Formatted(parent.pawn.Named("PAWN"), arg.Named("ORGANS"));
			TaggedString text = Props.letterDesc.Formatted(parent.pawn.Named("PAWN"), arg.Named("ORGANS"));
			Find.LetterStack.ReceiveLetter(label, text, Props.letterDef ?? LetterDefOf.NeutralEvent, parent.pawn);
		}
		parent.pawn.health.RemoveHediff(parent);
		added.Clear();
	}
}
