using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_SpawnMonolith : QuestPart
{
	private string inSignal;

	public QuestPart_SpawnMonolith()
	{
	}

	public QuestPart_SpawnMonolith(string inSignal)
	{
		this.inSignal = inSignal;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (!(signal.tag != inSignal))
		{
			Map randomSurfacePlayerHomeMap = Find.RandomSurfacePlayerHomeMap;
			if (randomSurfacePlayerHomeMap == null)
			{
				Log.Message("Tried to spawn monolith but no map found");
				quest.End(QuestEndOutcome.Fail);
			}
			else
			{
				Find.Anomaly.TryGetCellForMonolithSpawn(randomSurfacePlayerHomeMap, out var cell);
				GenSpawn.Spawn(ThingDefOf.MonolithIncoming, cell, randomSurfacePlayerHomeMap);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
	}
}
