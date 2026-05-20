using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_RemoveEquipmentFromPawns : QuestPart
{
	public List<Pawn> pawns = new List<Pawn>();

	public string inSignal;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		for (int i = 0; i < pawns.Count; i++)
		{
			if (pawns[i] != null && pawns[i].equipment != null)
			{
				pawns[i].equipment.DestroyAllEquipment();
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Values.Look(ref inSignal, "inSignal");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
