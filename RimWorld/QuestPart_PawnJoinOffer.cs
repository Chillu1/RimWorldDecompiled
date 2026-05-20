using UnityEngine;
using Verse;

namespace RimWorld;

public class QuestPart_PawnJoinOffer : QuestPartActivable
{
	public Pawn pawn;

	public bool letterSent;

	public string outSignalPawnAccepted;

	public string outSignalPawnRejected;

	public string letterLabel;

	public string letterText;

	public string letterTitle;

	public bool charity;

	public bool sendLetterOnEnable;

	private ChoiceLetter_AcceptVisitors letter;

	private const float MinMoodPercentage = 0.5f;

	private static readonly SimpleCurve JoinMTBbyMoodCurve = new SimpleCurve
	{
		new CurvePoint(0.5f, 60f),
		new CurvePoint(1f, 15f)
	};

	private const int CheckInterval = 2500;

	protected override void Enable(SignalArgs receivedArgs)
	{
		base.Enable(receivedArgs);
		if (sendLetterOnEnable)
		{
			SendLetter();
		}
	}

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		if (!sendLetterOnEnable && !letterSent && pawn != null && pawn.needs != null && pawn.needs.mood != null && pawn.IsHashIntervalTick(2500))
		{
			float curLevelPercentage = pawn.needs.mood.CurLevelPercentage;
			if (!(curLevelPercentage < 0.5f) && Rand.MTBEventOccurs(JoinMTBbyMoodCurve.Evaluate(curLevelPercentage), 60000f, 2500f))
			{
				SendLetter();
			}
		}
	}

	private void SendLetter()
	{
		if (!letterSent)
		{
			letter = (ChoiceLetter_AcceptVisitors)LetterMaker.MakeLetter(letterLabel, letterText, LetterDefOf.AcceptVisitors, null, quest);
			letter.title = letterTitle;
			letter.pawns.Add(pawn);
			letter.quest = quest;
			letter.acceptedSignal = outSignalPawnAccepted;
			letter.rejectedSignal = outSignalPawnRejected;
			letter.lookTargets = new LookTargets(pawn);
			letter.charity = charity;
			Find.LetterStack.ReceiveLetter(letter);
			letterSent = true;
			if (pawn.guest != null)
			{
				pawn.guest.Recruitable = true;
			}
		}
	}

	public override void Cleanup()
	{
		RemoveLetterIfActive();
	}

	protected override void Disable()
	{
		RemoveLetterIfActive();
	}

	private void RemoveLetterIfActive()
	{
		if (letter != null && Find.LetterStack.LettersListForReading.Contains(letter))
		{
			Find.LetterStack.RemoveLetter(letter);
		}
	}

	public override void DoDebugWindowContents(Rect innerRect, ref float curY)
	{
		if (base.State == QuestPartState.Enabled && !letterSent && pawn != null)
		{
			Rect rect = new Rect(innerRect.x, curY, 500f, 25f);
			if (Widgets.ButtonText(rect, $"Add Join Letter for {pawn.NameShortColored} " + GetType().Name))
			{
				SendLetter();
			}
			curY += rect.height + 4f;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref pawn, "pawn");
		Scribe_Values.Look(ref letterSent, "letterSent", defaultValue: false);
		Scribe_Values.Look(ref outSignalPawnAccepted, "outSignalPawnAccepted");
		Scribe_Values.Look(ref outSignalPawnRejected, "outSignalPawnRejected");
		Scribe_Values.Look(ref letterLabel, "letterLabel");
		Scribe_Values.Look(ref letterText, "letterText");
		Scribe_Values.Look(ref letterTitle, "letterTitle");
		Scribe_Values.Look(ref charity, "charity", defaultValue: false);
		Scribe_Values.Look(ref sendLetterOnEnable, "sendLetterOnEnable", defaultValue: false);
	}
}
