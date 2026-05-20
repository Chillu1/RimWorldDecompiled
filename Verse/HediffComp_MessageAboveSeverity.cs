using RimWorld;

namespace Verse;

public class HediffComp_MessageAboveSeverity : HediffComp
{
	private bool messageSent;

	protected HediffCompProperties_MessageAboveSeverity Props => (HediffCompProperties_MessageAboveSeverity)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (!messageSent && parent.Severity >= Props.severity)
		{
			if (PawnUtility.ShouldSendNotificationAbout(base.Pawn) && Props.messageType != null)
			{
				Messages.Message(Props.message.Formatted(base.Pawn), base.Pawn, Props.messageType);
			}
			messageSent = true;
		}
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref messageSent, "messageSent", defaultValue: false);
	}
}
