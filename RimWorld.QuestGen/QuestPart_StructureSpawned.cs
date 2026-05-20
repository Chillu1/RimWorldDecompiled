using RimWorld.Utility;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_StructureSpawned : QuestPart
{
	private const int VoidMetalPatchSize = 50;

	private static readonly IntRange NumMassesRange = new IntRange(3, 4);

	private string spawnedSignal;

	private Thing structure;

	public QuestPart_StructureSpawned()
	{
	}

	public QuestPart_StructureSpawned(string spawnedSignal, Thing structure)
	{
		this.spawnedSignal = spawnedSignal;
		this.structure = structure;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref spawnedSignal, "spawnedSignal");
		Scribe_References.Look(ref structure, "structure");
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (!(signal.tag != spawnedSignal))
		{
			VoidAwakeningUtility.SpawnVoidMetalAround(structure, 50, NumMassesRange.RandomInRange, withSkipEffects: true);
			VoidAwakeningUtility.SpawnMetalhorrorDefenders(structure);
		}
	}
}
