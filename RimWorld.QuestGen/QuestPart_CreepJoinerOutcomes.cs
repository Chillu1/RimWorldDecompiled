using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_CreepJoinerOutcomes : QuestPart
{
	public Pawn pawn;

	public string signalAccept;

	public string signalCapture;

	public string signalReject;

	public string signalAttacked;

	public string signalShow;

	public string signalTimeout;

	public int timeout;

	private ChoiceLetter_AcceptCreepJoiner letter;

	public void ShowOfferLetter(Pawn_CreepJoinerTracker creepjoiner)
	{
		if (letter != null)
		{
			letter.OpenLetter();
			return;
		}
		TaggedString label = "LetterCreeperInviteJoins".Translate(pawn.Named("PAWN"));
		TaggedString text = creepjoiner.form.letterPrompt.Formatted(pawn.Named("PAWN")).CapitalizeFirst();
		text += "\n\n" + creepjoiner.benefit.letterExtra.Formatted(pawn.Named("PAWN")).CapitalizeFirst();
		text += "\n\n" + "LetterCreeperInviteAppend".Translate(pawn.Named("PAWN")).CapitalizeFirst();
		letter = (ChoiceLetter_AcceptCreepJoiner)LetterMaker.MakeLetter(label, text, LetterDefOf.AcceptCreepJoiner, null, quest);
		letter.signalAccept = signalAccept;
		letter.signalCapture = signalCapture;
		letter.signalReject = signalReject;
		letter.pawn = pawn;
		letter.speaker = creepjoiner.speaker;
		Find.LetterStack.ReceiveLetter(letter);
		letter.OpenLetter();
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		Pawn_CreepJoinerTracker pawn_CreepJoinerTracker = pawn?.creepjoiner;
		if (pawn_CreepJoinerTracker != null)
		{
			if (signal.tag == signalShow)
			{
				ShowOfferLetter(pawn_CreepJoinerTracker);
			}
			else if (signal.tag == signalAttacked)
			{
				signal.args.TryGetArg("INSTIGATOR", out Pawn arg);
				pawn_CreepJoinerTracker.Notify_CreepJoinerAttacked(arg);
			}
			else if (signal.tag == signalReject)
			{
				pawn_CreepJoinerTracker.Notify_CreepJoinerRejected();
			}
			else if (signal.tag == signalTimeout)
			{
				pawn_CreepJoinerTracker.Notify_CreepJoinerRejected();
			}
			else if (signal.tag == signalCapture)
			{
				CaptureUtility.OrderArrest(pawn_CreepJoinerTracker.speaker, pawn);
			}
		}
	}

	public override void Cleanup()
	{
		CloseLetter();
	}

	private void CloseLetter()
	{
		if (letter != null && Find.LetterStack.LettersListForReading.Contains(letter))
		{
			Find.LetterStack.RemoveLetter(letter);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref pawn, "pawn");
		Scribe_References.Look(ref letter, "letter");
		Scribe_Values.Look(ref signalAccept, "signalAccept");
		Scribe_Values.Look(ref signalCapture, "signalCapture");
		Scribe_Values.Look(ref signalReject, "signalReject");
		Scribe_Values.Look(ref signalAttacked, "signalAttacked");
		Scribe_Values.Look(ref signalTimeout, "signalTimeout");
		Scribe_Values.Look(ref signalShow, "signalShow");
		Scribe_Values.Look(ref timeout, "timeout", 0);
	}
}
