using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_EnsureNotDowned : QuestPart
	{
		public string inSignal;

		public List<Pawn> pawns;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignal))
			{
				return;
			}
			foreach (Pawn pawn in pawns)
			{
				EnsureNotDowned(pawn);
			}
		}

		protected void EnsureNotDowned(Pawn p)
		{
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			int num = 0;
			while (p.Downed && num++ < 15)
			{
				Hediff hediff = hediffs.FirstOrDefault((Hediff h) => h.def.isBad && h.def.everCurableByItem && !(h is Hediff_MissingPart));
				if (hediff == null)
				{
					break;
				}
				p.health.RemoveHediff(hediff);
			}
			if (!p.Downed)
			{
				return;
			}
			num = 0;
			while (p.Downed && num++ < 15)
			{
				Hediff hediff2 = hediffs.FirstOrDefault((Hediff h) => h.def.isBad && !(h is Hediff_MissingPart));
				if (hediff2 != null)
				{
					p.health.RemoveHediff(hediff2);
					continue;
				}
				break;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn p) => p == null);
			}
		}
	}
}
