using Verse;

namespace RimWorld
{
	public class CompProperties_SnowExpand : CompProperties
	{
		public int expandInterval = 500;

		public float addAmount = 0.12f;

		public float maxRadius = 55f;

		public CompProperties_SnowExpand()
		{
			compClass = typeof(CompSnowExpand);
		}
	}
}
