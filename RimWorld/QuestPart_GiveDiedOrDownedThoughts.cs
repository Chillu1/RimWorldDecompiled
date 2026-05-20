using Verse;

namespace RimWorld
{
	public class QuestPart_GiveDiedOrDownedThoughts : QuestPart
	{
		public Pawn aboutPawn;

		public string inSignal;

		public PawnDiedOrDownedThoughtsKind thoughtsKind;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			if (signal.tag == inSignal)
			{
				PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(aboutPawn, null, thoughtsKind);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref aboutPawn, "aboutPawn");
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref thoughtsKind, "thoughtsKind", PawnDiedOrDownedThoughtsKind.Died);
		}
	}
}
