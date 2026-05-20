using RimWorld;

namespace Verse;

public class HediffComp_MessageStageIncreased : HediffComp
{
	private int lastStageMessaged;

	public bool sendMessages = true;

	protected HediffCompProperties_MessageStageIncreased Props => (HediffCompProperties_MessageStageIncreased)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (parent.CurStageIndex > lastStageMessaged)
		{
			lastStageMessaged = parent.CurStageIndex;
			if (sendMessages)
			{
				Messages.Message(Props.message.Formatted(base.Pawn), base.Pawn, Props.messageType ?? MessageTypeDefOf.NeutralEvent);
			}
		}
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref sendMessages, "sendMessages", defaultValue: false);
		Scribe_Values.Look(ref lastStageMessaged, "lastStageMessaged", 0);
	}
}
