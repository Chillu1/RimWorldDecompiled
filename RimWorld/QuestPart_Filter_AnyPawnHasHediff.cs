using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_Filter_AnyPawnHasHediff : QuestPart_Filter
{
	public List<Pawn> pawns;

	public HediffDef hediff;

	public string inSignalRemovePawn;

	protected override bool Pass(SignalArgs args)
	{
		if (pawns.NullOrEmpty())
		{
			return false;
		}
		foreach (Pawn pawn in pawns)
		{
			if (pawn != null && pawn.health.hediffSet.HasHediff(hediff))
			{
				return true;
			}
		}
		return false;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!inSignalRemovePawn.NullOrEmpty() && signal.tag == inSignalRemovePawn && signal.args.TryGetArg("SUBJECT", out Pawn arg) && pawns.Contains(arg))
		{
			pawns.Remove(arg);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Defs.Look(ref hediff, "hediff");
		Scribe_Values.Look(ref inSignalRemovePawn, "inSignalRemovePawn");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
