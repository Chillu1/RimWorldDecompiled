using RimWorld;

namespace Verse;

public class HediffComp_Discoverable : HediffComp
{
	private bool discovered;

	private const int CheckInterval = 103;

	public HediffCompProperties_Discoverable Props => (HediffCompProperties_Discoverable)props;

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref discovered, "discovered", defaultValue: false);
	}

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (parent.pawn.IsHashIntervalTick(103, delta))
		{
			CheckDiscovered();
		}
	}

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		CheckDiscovered();
	}

	private void CheckDiscovered()
	{
		if (discovered || !parent.Visible)
		{
			return;
		}
		discovered = true;
		if (!Props.sendLetterWhenDiscovered || !PawnUtility.ShouldSendNotificationAbout(base.Pawn))
		{
			return;
		}
		if (base.Pawn.RaceProps.Humanlike)
		{
			string text = (Props.discoverLetterLabel.NullOrEmpty() ? ((string)("LetterLabelNewDisease".Translate() + ": " + base.Def.LabelCap)) : string.Format(Props.discoverLetterLabel, base.Pawn.LabelShortCap).CapitalizeFirst());
			string text2 = ((!Props.discoverLetterText.NullOrEmpty()) ? ((string)Props.discoverLetterText.Formatted(base.Pawn.LabelIndefinite(), base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst()) : ((parent.Part != null) ? ((string)"NewPartDisease".Translate(base.Pawn.Named("PAWN"), parent.Part.Label, base.Pawn.LabelDefinite(), base.Def.label).AdjustedFor(base.Pawn).CapitalizeFirst()) : ((string)"NewDisease".Translate(base.Pawn.Named("PAWN"), base.Def.label, base.Pawn.LabelDefinite()).AdjustedFor(base.Pawn).CapitalizeFirst())));
			Find.LetterStack.ReceiveLetter(text, text2, (Props.letterType != null) ? Props.letterType : LetterDefOf.NegativeEvent, base.Pawn);
			return;
		}
		string text3;
		if (Props.discoverLetterText.NullOrEmpty())
		{
			text3 = ((parent.Part != null) ? ((string)"NewPartDiseaseAnimal".Translate(base.Pawn.LabelShort, parent.Part.Label, base.Pawn.LabelDefinite(), base.Def.LabelCap, base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst()) : ((string)"NewDiseaseAnimal".Translate(base.Pawn.LabelShort, base.Def.LabelCap, base.Pawn.LabelDefinite(), base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst()));
		}
		else
		{
			string text4 = base.Pawn.KindLabelIndefinite();
			if (base.Pawn.Name.IsValid && !base.Pawn.Name.Numerical)
			{
				text4 = base.Pawn.Name?.ToString() + " (" + base.Pawn.KindLabel + ")";
			}
			text3 = Props.discoverLetterText.Formatted(text4, base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst();
		}
		Messages.Message(text3, base.Pawn, (Props.messageType != null) ? Props.messageType : MessageTypeDefOf.NegativeHealthEvent);
	}

	public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		CheckDiscovered();
	}

	public override string CompDebugString()
	{
		return "discovered: " + discovered;
	}
}
