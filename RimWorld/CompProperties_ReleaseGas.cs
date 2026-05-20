using Verse;

namespace RimWorld
{
	public class CompProperties_ReleaseGas : CompProperties
	{
		public GasType gasType;

		public float cellsToFill;

		public float durationSeconds;

		public EffecterDef effecterReleasing;

		public CompProperties_ReleaseGas()
		{
			compClass = typeof(CompReleaseGas);
		}
	}
}
