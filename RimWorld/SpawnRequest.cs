using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class SpawnRequest : IExposable
{
	public List<Thing> thingsToSpawn;

	public List<Thing> spawnedThings = new List<Thing>();

	public List<Thing> unspawnedThings;

	public List<IntVec3> spawnPositions;

	public DeferredSpawnWorker spawnWorker;

	public Lord lord;

	public int batchSize;

	public int intervalTicks;

	public int initialDelay;

	public int startedTick;

	public bool done;

	public SoundDef spawnSound;

	public EffecterDef spawnEffect;

	public EffecterDef preSpawnEffect;

	public int preSpawnEffecterOffsetTicks;

	public int PreEffecterTick => startedTick + initialDelay + preSpawnEffecterOffsetTicks;

	public SpawnRequest()
	{
	}

	public SpawnRequest(List<Thing> thingsToSpawn, List<IntVec3> spawnPositions, int batchSize, float intervalSeconds, Lord lord = null)
	{
		this.thingsToSpawn = thingsToSpawn;
		unspawnedThings = thingsToSpawn.ToList();
		this.spawnPositions = spawnPositions;
		this.batchSize = batchSize;
		this.lord = lord;
		intervalTicks = intervalSeconds.SecondsToTicks();
		startedTick = Find.TickManager.TicksGame;
	}

	public SpawnRequest(List<Thing> thingsToSpawn, int batchSize, float intervalSeconds, Lord lord = null)
		: this(thingsToSpawn, thingsToSpawn.Select((Thing t) => t.Position).ToList(), batchSize, intervalSeconds, lord)
	{
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref unspawnedThings, "unspawnedThings", LookMode.Deep);
		Scribe_Collections.Look(ref thingsToSpawn, "thingsToSpawn", LookMode.Reference);
		Scribe_Collections.Look(ref spawnPositions, "spawnPositions", LookMode.Value);
		Scribe_Collections.Look(ref spawnedThings, "spawnedThings", LookMode.Reference);
		Scribe_Deep.Look(ref spawnWorker, "spawnWorker");
		Scribe_Values.Look(ref batchSize, "batchSize", 0);
		Scribe_Values.Look(ref intervalTicks, "intervalTicks", 0);
		Scribe_Values.Look(ref initialDelay, "initialDelay", 0);
		Scribe_Values.Look(ref startedTick, "startedTick", 0);
		Scribe_References.Look(ref lord, "lord");
		Scribe_Values.Look(ref preSpawnEffecterOffsetTicks, "effecterOffsetTicks", 0);
	}
}
