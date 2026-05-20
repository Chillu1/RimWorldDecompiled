using Verse;

namespace RimWorld
{
	public class CompProperties_Dissolution : CompProperties
	{
		public int dissolutionAfterDays = 4;

		public float dissolutinFactorIndoors = 0.5f;

		public float dissolutionFactorRain = 2f;

		public CompProperties_Dissolution()
		{
			compClass = typeof(CompDissolution);
		}
	}
}
