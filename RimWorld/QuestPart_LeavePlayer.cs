using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_LeavePlayer : QuestPart
{
	public string inSignal;

	public List<Pawn> pawns = new List<Pawn>();

	public Faction replacementFaction;

	public string inSignalRemovePawn;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignal)
		{
			pawns.RemoveAll((Pawn x) => x.Destroyed);
			for (int num = 0; num < pawns.Count; num++)
			{
				if (pawns[num].Faction == Faction.OfPlayer)
				{
					pawns[num].SetFaction(replacementFaction);
				}
			}
		}
		if (signal.tag == inSignalRemovePawn && signal.args.TryGetArg("SUBJECT", out Pawn arg) && pawns.Contains(arg))
		{
			pawns.Remove(arg);
		}
	}

	public override void Notify_FactionRemoved(Faction faction)
	{
		if (replacementFaction == faction)
		{
			replacementFaction = null;
		}
	}

	public override bool QuestPartReserves(Pawn p)
	{
		return pawns.Contains(p);
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_References.Look(ref replacementFaction, "replacementFaction");
		Scribe_Values.Look(ref inSignalRemovePawn, "inSignalRemovePawn");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
