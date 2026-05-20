using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_GiveMemoryToHumansOnMap : QuestPart
{
	public ThoughtDef memory;

	public string inSignal;

	public MapParent mapParent;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (!(signal.tag == inSignal) || mapParent?.Map == null)
		{
			return;
		}
		foreach (Pawn item in mapParent.Map.mapPawns.AllPawnsSpawned)
		{
			item.needs?.mood?.thoughts?.memories?.TryGainMemory(memory);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref memory, "memory");
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref mapParent, "mapParent");
	}
}
