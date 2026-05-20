using Verse;

namespace RimWorld
{
	public class CompProperties_SpawnEffectersInRoom : CompProperties
	{
		public EffecterDef effecter;

		public float radius = 10f;

		public CompProperties_SpawnEffectersInRoom()
		{
			compClass = typeof(CompSpawnEffectersInRoom);
		}
	}
}
