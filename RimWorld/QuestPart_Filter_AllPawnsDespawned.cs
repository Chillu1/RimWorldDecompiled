using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_Filter_AllPawnsDespawned : QuestPart_Filter
{
	public List<Pawn> pawns;

	public string inSignalRemovePawn;

	protected override bool Pass(SignalArgs args)
	{
		if (pawns.NullOrEmpty())
		{
			return false;
		}
		foreach (Pawn pawn in pawns)
		{
			if (pawn.Spawned)
			{
				return false;
			}
		}
		return true;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag == inSignalRemovePawn && signal.args.TryGetArg("SUBJECT", out Pawn arg) && pawns.Contains(arg))
		{
			pawns.Remove(arg);
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
