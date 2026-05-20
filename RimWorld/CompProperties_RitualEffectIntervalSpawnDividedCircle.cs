namespace RimWorld
{
	public class CompProperties_RitualEffectIntervalSpawnDividedCircle : CompProperties_RitualEffectIntervalSpawn
	{
		public float radius = 5f;

		public int numCopies = 5;

		public CompProperties_RitualEffectIntervalSpawnDividedCircle()
		{
			compClass = typeof(CompRitualEffect_IntervalSpawnDividedCircleEffecter);
		}
	}
}
