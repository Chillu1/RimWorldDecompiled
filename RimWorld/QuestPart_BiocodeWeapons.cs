using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_BiocodeWeapons : QuestPart
{
	public string inSignal;

	public List<Pawn> pawns = new List<Pawn>();

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		for (int i = 0; i < pawns.Count; i++)
		{
			if (pawns[i].equipment == null)
			{
				continue;
			}
			foreach (ThingWithComps item in pawns[i].equipment.AllEquipmentListForReading)
			{
				CompBiocodable comp = item.GetComp<CompBiocodable>();
				if (comp != null && !comp.Biocoded)
				{
					comp.CodeFor(pawns[i]);
				}
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}
}
