using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class QuestPart_TransporterPawns : QuestPart
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
				Process(pawn2);
			}
		}
		if (pawnsInTransporter == null)
		{
			return;
		}
		foreach (Thing item in (IEnumerable<Thing>)pawnsInTransporter.TryGetComp<CompTransporter>().innerContainer)
		{
			if (item is Pawn pawn)
			{
				Process(pawn);
			}
		}
	}

	public abstract void Process(Pawn pawn);

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
