using Verse;

namespace RimWorld;

public class GenStep_PitBurrows : GenStep
{
	private static readonly IntRange NumBurrowsRange = new IntRange(2, 3);

	public override int SeedPart => 512346123;

	public override void Generate(Map map, GenStepParams parms)
	{
		int randomInRange = NumBurrowsRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			if (RCellFinder.TryFindRandomCellNearWith(map.Center, (IntVec3 c) => GenSpawn.CanSpawnAt(ThingDefOf.PitBurrow, c, map), map, out var result, 30))
			{
				GenSpawn.Spawn(ThingDefOf.PitBurrow, result, map).SetFaction(Faction.OfEntities);
			}
		}
	}

	private bool Validator(CellRect rect, Map map)
	{
		foreach (IntVec3 cell in rect.Cells)
		{
			if (!cell.Standable(map))
			{
				return false;
			}
			if (!cell.GetAffordances(map).Contains(ThingDefOf.PitBurrow.terrainAffordanceNeeded))
			{
				return false;
			}
		}
		return true;
	}
}
