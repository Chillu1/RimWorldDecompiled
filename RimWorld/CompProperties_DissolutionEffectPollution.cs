using Verse;

namespace RimWorld
{
	public class CompProperties_DissolutionEffectPollution : CompProperties
	{
		public int cellsToPollutePerDissolution = 4;

		public float tilePollutionPerDissolution = 0.0005f;

		public float waterTilePollutionFactor = 8f;

		public SimpleCurve goodWillFactorOverDistanceCurce;

		public CompProperties_DissolutionEffectPollution()
		{
			compClass = typeof(CompDissolutionEffect_Pollution);
		}
	}
}
