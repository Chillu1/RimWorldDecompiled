using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class QuestPart_Filter_AnyPawn : QuestPart_Filter
	{
		public List<Pawn> pawns;

		public string inSignalRemovePawn;

		protected abstract int PawnsCount { get; }

		protected override bool Pass(SignalArgs args)
		{
			return PawnsCount > 0;
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			if (signal.tag == inSignalRemovePawn && signal.args.TryGetArg("SUBJECT", out Pawn arg) && pawns.Contains(arg))
			{
				pawns.Remove(arg);
			}
			if (signal.tag == inSignal)
			{
				signal.args.Add(PawnsCount.Named("PAWNSALIVECOUNT"));
			}
			base.Notify_QuestSignalReceived(signal);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			Scribe_Values.Look(ref inSignalRemovePawn, "inSignalRemovePawn");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}
	}
}
