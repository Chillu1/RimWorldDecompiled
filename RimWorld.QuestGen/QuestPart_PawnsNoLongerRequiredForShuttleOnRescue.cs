using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestPart_PawnsNoLongerRequiredForShuttleOnRescue : QuestPart
	{
		public string inSignal;

		public Thing shuttle;

		public List<Pawn> pawns = new List<Pawn>();

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			if (signal.tag == inSignal && signal.args.TryGetArg("SUBJECT", out Pawn arg) && pawns.Contains(arg))
			{
				shuttle.TryGetComp<CompShuttle>()?.requiredPawns.Remove(arg);
				pawns.Remove(arg);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref shuttle, "shuttle");
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}
	}
}
