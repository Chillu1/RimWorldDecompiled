using Verse;

namespace RimWorld;

public class GenStep_Animals : GenStep
{
	public override int SeedPart => 1298760307;

	public override void Generate(Map map, GenStepParams parms)
	{
		int num = 0;
		while (!map.wildAnimalSpawner.AnimalEcosystemFull)
		{
			num++;
			if (num >= 10000)
			{
				Log.Error("Too many iterations.");
				break;
			}
			IntVec3 loc = RCellFinder.RandomAnimalSpawnCell_MapGen(map);
			if (!map.wildAnimalSpawner.SpawnRandomWildAnimalAt(loc, canFlyIn: false))
			{
				break;
			}
		}
	}
}
