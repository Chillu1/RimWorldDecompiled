using Verse;

namespace RimWorld
{
	public class CompProperties_PlantDamager : CompProperties
	{
		public int ticksBetweenDamage;

		public float radius;

		public bool ignoreAnima = true;

		public float damagePerCycle;

		public int cycleCountOnSpawn;

		public CompProperties_PlantDamager()
		{
			compClass = typeof(CompPlantDamager);
		}
	}
}
