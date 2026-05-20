using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class LordJob_Joinable_Speech : LordJob_Ritual
{
	private bool titleSpeech;

	protected override int MinTicksToFinish => base.DurationTicks / 2;

	public override bool AllowStartNewGatherings => false;

	public override bool OrganizerIsStartingPawn => true;

	public LordJob_Joinable_Speech()
	{
	}

	public LordJob_Joinable_Speech(TargetInfo spot, Pawn organizer, Precept_Ritual ritual, List<RitualStage> stages, RitualRoleAssignments assignments, bool titleSpeech)
		: base(spot, ritual, null, stages, assignments, organizer)
	{
		Building_Throne firstThing = spot.Cell.GetFirstThing<Building_Throne>(organizer.Map);
		if (firstThing != null)
		{
			selectedTarget = firstThing;
		}
		this.titleSpeech = titleSpeech;
	}

	protected override LordToil_Ritual MakeToil(RitualStage stage)
	{
		if (stage == null)
		{
			return new LordToil_Speech(spot, ritual, this, organizer);
		}
		return new LordToil_Ritual(spot, this, stage, organizer);
	}

	public override string GetReport(Pawn pawn)
	{
		return ((pawn != organizer) ? "LordReportListeningSpeech".Translate(organizer.Named("ORGANIZER")) : "LordReportGivingSpeech".Translate()) + base.TimeLeftPostfix;
	}

	public override void ApplyOutcome(float progress, bool showFinishedMessage = true, bool showFailedMessage = true, bool cancelled = false)
	{
		if (ticksPassed < MinTicksToFinish || cancelled)
		{
			Find.LetterStack.ReceiveLetter("LetterLabelSpeechCancelled".Translate(), "LetterSpeechCancelled".Translate(titleSpeech ? GrammarResolverSimple.PawnResolveBestRoyalTitle(organizer) : organizer.LabelShort).CapitalizeFirst(), LetterDefOf.NegativeEvent, organizer);
			ritual.outcomeEffect?.ResetCompDatas();
		}
		else
		{
			base.ApplyOutcome(progress, showFinishedMessage: false);
		}
	}
}
