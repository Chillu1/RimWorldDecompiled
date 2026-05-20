using Verse;

namespace RimWorld;

public class GameCondition_VolcanicDebrisShower : GameCondition
{
	private static readonly IntRange MeteoriteCount = new IntRange(8, 12);

	private static readonly IntRange IntervalRange = new IntRange(60, 180);

	private int nextLavaRockTick;

	private int remainingLavaRockCount;

	public override bool Expired => remainingLavaRockCount <= 0;

	public override void Init()
	{
		base.Init();
		nextLavaRockTick = Find.TickManager.TicksGame + IntervalRange.RandomInRange;
		remainingLavaRockCount = MeteoriteCount.RandomInRange;
	}

	public override void GameConditionTick()
	{
		if (Find.TickManager.TicksGame >= nextLavaRockTick)
		{
			Map singleMap = base.SingleMap;
			if (!TryFindCell(out var cell, singleMap))
			{
				Log.Error("GameCondition_VolcanicDebrisShower could not find a suitable cell.");
				return;
			}
			SkyfallerMaker.SpawnSkyfaller(ThingDefOf.LavaRockIncoming, cell, singleMap);
			nextLavaRockTick = Find.TickManager.TicksGame + IntervalRange.RandomInRange;
			remainingLavaRockCount--;
		}
	}

	private static bool TryFindCell(out IntVec3 cell, Map map)
	{
		return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.LavaRockIncoming, map, TerrainAffordanceDefOf.Walkable, out cell, 10, default(IntVec3), -1, allowRoofedCells: true, allowCellsWithItems: false, allowCellsWithBuildings: false, colonyReachable: false, avoidColonistsIfExplosive: true, alwaysAvoidColonists: true);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref nextLavaRockTick, "nextLavaRockTick", 0);
		Scribe_Values.Look(ref remainingLavaRockCount, "remainingLavaRockCount", 0);
	}
}
