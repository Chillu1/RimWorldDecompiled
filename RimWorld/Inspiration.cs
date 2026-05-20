using Verse;

namespace RimWorld;

public class Inspiration : IExposable
{
	public Pawn pawn;

	public InspirationDef def;

	private int age;

	public string reason;

	public int Age => age;

	public float AgeDays => (float)age / 60000f;

	public virtual string InspectLine
	{
		get
		{
			int numTicks = (int)((def.baseDurationDays - AgeDays) * 60000f);
			return def.baseInspectLine + " (" + "ExpiresIn".Translate() + ": " + numTicks.ToStringTicksToPeriod() + ")";
		}
	}

	public virtual string LetterText
	{
		get
		{
			string text = def.beginLetter.Formatted(pawn.LabelCap, pawn.Named("PAWN")).AdjustedFor(pawn).Resolve();
			if (!string.IsNullOrWhiteSpace(reason))
			{
				text = reason.Formatted(pawn.LabelCap, pawn.Named("PAWN")).AdjustedFor(pawn).Resolve() + "\n\n" + text;
			}
			return text;
		}
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref age, "age", 0);
		Scribe_Values.Look(ref reason, "reason");
	}

	public virtual void InspirationTick(int delta)
	{
		age += delta;
		if (AgeDays >= def.baseDurationDays)
		{
			End();
		}
	}

	public virtual void PostStart(bool sendLetter = true)
	{
		if (sendLetter)
		{
			SendBeginLetter();
		}
	}

	public virtual void PostEnd()
	{
		AddEndMessage();
	}

	protected void End()
	{
		pawn.mindState.inspirationHandler.EndInspiration(this);
	}

	protected virtual void SendBeginLetter()
	{
		if (!def.beginLetter.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			string text = (def.beginLetterLabel ?? ((string)def.LabelCap)).CapitalizeFirst() + ": " + pawn.LabelShortCap;
			Find.LetterStack.ReceiveLetter(text, LetterText, def.beginLetterDef, pawn);
		}
	}

	protected virtual void AddEndMessage()
	{
		if (!def.endMessage.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			Messages.Message(def.endMessage.Formatted(pawn.LabelCap, pawn.Named("PAWN")).AdjustedFor(pawn), pawn, MessageTypeDefOf.NeutralEvent);
		}
	}
}
