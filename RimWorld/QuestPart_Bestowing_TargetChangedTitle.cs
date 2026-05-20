using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class QuestPart_Bestowing_TargetChangedTitle : QuestPart
	{
		public string inSignal;

		public Pawn pawn;

		public Pawn bestower;

		public RoyalTitleDef currentTitle;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				string text = null;
				string text2 = null;
				LetterDef letterDef = null;
				RoyalTitleDef titleAwardedWhenUpdating = pawn.royalty.GetTitleAwardedWhenUpdating(bestower.Faction, pawn.royalty.GetFavor(bestower.Faction));
				if (titleAwardedWhenUpdating != null && titleAwardedWhenUpdating.seniority > currentTitle.seniority)
				{
					text2 = "LetterLabelBestowingCeremonyTitleUpdated";
					text = "LetterTextBestowingCeremonyTitleUpdated";
					letterDef = LetterDefOf.NeutralEvent;
					SoundDefOf.Quest_Concluded.PlayOneShotOnCamera();
				}
				else
				{
					text2 = "LetterQuestFailedLabel";
					text = "LetterQuestCompletedFail";
					letterDef = LetterDefOf.NegativeEvent;
					SoundDefOf.Quest_Failed.PlayOneShotOnCamera();
				}
				Find.LetterStack.ReceiveLetter(text2.Translate(pawn.Named("TARGET")), text.Translate(quest.name.CapitalizeFirst(), pawn.Named("TARGET")), letterDef, pawn, null, quest);
				quest.End(QuestEndOutcome.Unknown, sendLetter: false);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref pawn, "pawn");
			Scribe_References.Look(ref bestower, "bestower");
			Scribe_Defs.Look(ref currentTitle, "currentTitle");
		}
	}
}
