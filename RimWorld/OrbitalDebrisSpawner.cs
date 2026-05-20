using Verse;

namespace RimWorld;

public class OrbitalDebrisSpawner : Thing
{
	private static readonly FloatRange DelaySecondsRange = new FloatRange(3f, 12f);

	private static readonly FloatRange DurationSecondsRange = new FloatRange(5f, 5f);

	private static readonly IntRange MeteoriteRange = new IntRange(12, 16);

	private int startTick;

	private int endTick;

	private int meteoriteCount;

	private int meteoritesSpawned;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref startTick, "startTick", 0);
		Scribe_Values.Look(ref endTick, "endTick", 0);
		Scribe_Values.Look(ref meteoriteCount, "meteoriteCount", 0);
		Scribe_Values.Look(ref meteoritesSpawned, "meteoritesSpawned", 0);
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			startTick = GenTicks.TicksGame + DelaySecondsRange.RandomInRange.SecondsToTicks();
			endTick = startTick + DurationSecondsRange.RandomInRange.SecondsToTicks();
			meteoriteCount = MeteoriteRange.RandomInRange;
		}
	}

	protected override void Tick()
	{
		if (GenTicks.TicksGame > endTick)
		{
			Destroy();
		}
		else if (GenTicks.TicksGame >= startTick && meteoritesSpawned < meteoriteCount)
		{
			int positionsRemaining = endTick - GenTicks.TicksGame;
			if (Rand.DynamicChance(meteoritesSpawned, meteoriteCount, positionsRemaining))
			{
				meteoritesSpawned++;
				SpawnMeteoriteIncoming();
			}
		}
	}

	private void SpawnMeteoriteIncoming()
	{
		if (TryFindCell(out var cell))
		{
			SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteCraterIncoming, cell, base.Map);
		}
	}

	private bool TryFindCell(out IntVec3 cell)
	{
		return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteCraterIncoming, base.Map, TerrainAffordanceDefOf.Light, out cell, 10, default(IntVec3), -1, allowRoofedCells: false, allowCellsWithItems: false, allowCellsWithBuildings: false, colonyReachable: false, avoidColonistsIfExplosive: false);
	}
}
