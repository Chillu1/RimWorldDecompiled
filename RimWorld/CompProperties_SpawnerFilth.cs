using Verse;

namespace RimWorld
{
	public class CompProperties_SpawnerFilth : CompProperties
	{
		public ThingDef filthDef;

		public int spawnCountOnSpawn = 5;

		public float spawnMtbHours = 12f;

		public float spawnRadius = 3f;

		public float spawnEveryDays = -1f;

		public RotStage? requiredRotStage;

		public CompProperties_SpawnerFilth()
		{
			compClass = typeof(CompSpawnerFilth);
		}
	}
}
