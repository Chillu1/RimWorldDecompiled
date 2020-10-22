using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_AnyPawnAlive : QuestPart_Filter
	{
		public List<Pawn> pawns;

		public string inSignalRemovePawn;

		private int PawnsAliveCount
		{
			get
			{
				if (pawns.NullOrEmpty())
				{
					return 0;
				}
				int num = 0;
				for (int i = 0; i < pawns.Count; i++)
				{
					if (!pawns[i].Destroyed)
					{
						num++;
					}
				}
				return num;
			}
		}

		protected override bool Pass(SignalArgs args)
		{
			return PawnsAliveCount > 0;
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			if (signal.tag == inSignalRemovePawn && signal.args.TryGetArg("SUBJECT", out Pawn arg) && pawns.Contains(arg))
			{
				pawns.Remove(arg);
			}
			if (signal.tag == inSignal)
			{
				signal.args.Add(PawnsAliveCount.Named("PAWNSALIVECOUNT"));
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
