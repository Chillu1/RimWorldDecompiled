using RimWorld;

namespace Verse;

public class HediffComp_MessageAfterTicks : HediffComp
{
	private int ticksUntilMessage;

	protected HediffCompProperties_MessageAfterTicks Props => (HediffCompProperties_MessageAfterTicks)props;

	public override void CompPostMake()
	{
		base.CompPostMake();
		ticksUntilMessage = Props.ticks;
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (ticksUntilMessage == 0)
		{
			if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
			{
				if (Props.messageType != null)
				{
					Messages.Message(Props.message.Formatted(base.Pawn), base.Pawn, Props.messageType);
				}
				if (Props.letterType != null)
				{
					Find.LetterStack.ReceiveLetter(Props.letterLabel.Formatted(base.Pawn), GetLetterText(), Props.letterType, base.Pawn);
				}
			}
			ticksUntilMessage--;
		}
		else if (ticksUntilMessage > 0)
		{
			ticksUntilMessage--;
		}
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref ticksUntilMessage, "ticksUntilMessage", 0);
	}

	private TaggedString GetLetterText()
	{
		if (parent is Hediff_Pregnant { Mother: not null } hediff_Pregnant && hediff_Pregnant.Mother != hediff_Pregnant.pawn)
		{
			TaggedString result = "IvfPregnancyLetterText".Translate(hediff_Pregnant.pawn.NameFullColored);
			if (hediff_Pregnant.Mother != null && hediff_Pregnant.Father != null)
			{
				result += "\n\n" + "IvfPregnancyLetterParents".Translate(hediff_Pregnant.Mother.NameFullColored, hediff_Pregnant.Father.NameFullColored);
			}
			return result;
		}
		return Props.letterText.Formatted(base.Pawn);
	}
}
