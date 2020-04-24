using Verse;

namespace RimWorld
{
	public class SignalAction_Letter : SignalAction
	{
		public Letter letter;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref letter, "letter");
		}

		protected override void DoAction(SignalArgs args)
		{
			if (args.TryGetArg("SUBJECT", out Pawn arg))
			{
				ChoiceLetter choiceLetter = letter as ChoiceLetter;
				if (choiceLetter != null)
				{
					choiceLetter.text = choiceLetter.text.Resolve().Formatted(arg.LabelShort, arg.Named("PAWN")).AdjustedFor(arg);
				}
				if (!letter.lookTargets.IsValid())
				{
					letter.lookTargets = arg;
				}
			}
			Find.LetterStack.ReceiveLetter(letter);
		}
	}
}
