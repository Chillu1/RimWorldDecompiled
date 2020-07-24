using Verse;

namespace RimWorld
{
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

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref age, "age", 0);
			Scribe_Values.Look(ref reason, "reason");
		}

		public virtual void InspirationTick()
		{
			age++;
			if (AgeDays >= def.baseDurationDays)
			{
				End();
			}
		}

		public virtual void PostStart()
		{
			SendBeginLetter();
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
				TaggedString taggedString = def.beginLetter.Formatted(pawn.LabelCap, pawn.Named("PAWN")).AdjustedFor(pawn);
				if (!string.IsNullOrWhiteSpace(reason))
				{
					taggedString = reason.Formatted(pawn.LabelCap, pawn.Named("PAWN")).AdjustedFor(pawn) + "\n\n" + taggedString;
				}
				string str = (def.beginLetterLabel ?? ((string)def.LabelCap)).CapitalizeFirst() + ": " + pawn.LabelShortCap;
				Find.LetterStack.ReceiveLetter(str, taggedString, def.beginLetterDef, pawn);
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
}
