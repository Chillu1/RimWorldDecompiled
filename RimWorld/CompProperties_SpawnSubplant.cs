using Verse;

namespace RimWorld
{
	public class CompProperties_SpawnSubplant : CompProperties
	{
		public ThingDef subplant;

		public SoundDef spawnSound;

		public CompProperties_SpawnSubplant()
		{
			compClass = typeof(CompSpawnSubplant);
		}
	}
}
