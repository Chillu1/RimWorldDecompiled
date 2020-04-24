using Verse;

namespace RimWorld
{
	public class CompProperties_SpawnerHives : CompProperties
	{
		public float HiveSpawnPreferredMinDist = 3.5f;

		public float HiveSpawnRadius = 10f;

		public FloatRange HiveSpawnIntervalDays = new FloatRange(2f, 3f);

		public SimpleCurve ReproduceRateFactorFromNearbyHiveCountCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(7f, 0.35f)
		};

		public CompProperties_SpawnerHives()
		{
			compClass = typeof(CompSpawnerHives);
		}
	}
}
