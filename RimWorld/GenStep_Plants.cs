using Verse;

namespace RimWorld
{
	public class GenStep_Plants : GenStep
	{
		private const float ChanceToSkip = 0.001f;

		public override int SeedPart => 578415222;

		public override void Generate(Map map, GenStepParams parms)
		{
			map.regionAndRoomUpdater.Enabled = false;
			float currentPlantDensity = map.wildPlantSpawner.CurrentPlantDensity;
			float currentWholeMapNumDesiredPlants = map.wildPlantSpawner.CurrentWholeMapNumDesiredPlants;
			foreach (IntVec3 item in map.cellsInRandomOrder.GetAll())
			{
				if (!Rand.Chance(0.001f))
				{
					map.wildPlantSpawner.CheckSpawnWildPlantAt(item, currentPlantDensity, currentWholeMapNumDesiredPlants, setRandomGrowth: true);
				}
			}
			map.regionAndRoomUpdater.Enabled = true;
		}
	}
}
