using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_ChangeGoodwillForAlivePawnsMissingFromShuttle : QuestPart
{
	public string inSignal;

	public List<Pawn> pawns;

	public Faction faction;

	public int goodwillChange;

	public HistoryEventDef historyEvent;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag == inSignal && signal.args.TryGetArg("SENT", out IEnumerable<Thing> arg))
		{
			DoWork(arg);
		}
	}

	private void DoWork(IEnumerable<Thing> sentThings)
	{
		int num = 0;
		foreach (Thing sentThing in sentThings)
		{
			if (sentThing is Pawn)
			{
				num++;
			}
		}
		int num2 = 0;
		foreach (Pawn pawn in pawns)
		{
			if (!pawn.Dead)
			{
				num2++;
			}
		}
		if (num < num2)
		{
			Faction.OfPlayer.TryAffectGoodwillWith(faction, goodwillChange * (num2 - num), canSendMessage: true, canSendHostilityLetter: true, historyEvent);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref goodwillChange, "goodwillChange", 0);
		Scribe_Defs.Look(ref historyEvent, "historyEvent");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
