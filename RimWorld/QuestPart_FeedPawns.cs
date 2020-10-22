using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_FeedPawns : QuestPart
	{
		public string inSignal;

		public Thing pawnsInTransporter;

		public List<Pawn> pawns = new List<Pawn>();

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			if (!(signal.tag == inSignal))
			{
				return;
			}
			if (pawns != null)
			{
				foreach (Pawn pawn2 in pawns)
				{
					pawn2.needs.food.CurLevel = pawn2.needs.food.MaxLevel;
				}
			}
			if (pawnsInTransporter == null)
			{
				return;
			}
			foreach (Thing item in (IEnumerable<Thing>)pawnsInTransporter.TryGetComp<CompTransporter>().innerContainer)
			{
				Pawn pawn;
				if ((pawn = item as Pawn) != null)
				{
					pawn.needs.food.CurLevel = pawn.needs.food.MaxLevel;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			Scribe_References.Look(ref pawnsInTransporter, "pawnsInTransporter");
			Scribe_Values.Look(ref inSignal, "inSignal");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void ReplacePawnReferences(Pawn replace, Pawn with)
		{
			if (pawns != null)
			{
				pawns.Replace(replace, with);
			}
		}
	}
}
