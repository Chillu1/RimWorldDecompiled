using RimWorld.Utility;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_SetMonolithGleaming : QuestPart
{
	private const int VoidMetalPatchSize = 300;

	private static readonly IntRange NumMassesRange = new IntRange(3, 4);

	private Building_VoidMonolith monolith;

	private string inSignal;

	public QuestPart_SetMonolithGleaming()
	{
	}

	public QuestPart_SetMonolithGleaming(Building_VoidMonolith monolith, string inSignal)
	{
		this.monolith = monolith;
		this.inSignal = inSignal;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (!(signal.tag != inSignal))
		{
			Find.Anomaly.SetLevel(MonolithLevelDefOf.Gleaming, silent: true);
			CellRect cellRect = monolith.OccupiedRect();
			VoidAwakeningUtility.SpawnVoidMetalAround(avoidRect: new CellRect(cellRect.minX, cellRect.minZ - 3, 3, 3), thing: monolith, size: 300, numMasses: NumMassesRange.RandomInRange, withSkipEffects: true);
			VoidAwakeningUtility.SpawnMetalhorrorDefenders(monolith);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref monolith, "monolith");
		Scribe_Values.Look(ref inSignal, "inSignal");
	}
}
