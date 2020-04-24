using Verse;

namespace RimWorld
{
	public class CompProperties_TargetEffect_GoodwillImpact : CompProperties
	{
		public int goodwillImpact = -200;

		public CompProperties_TargetEffect_GoodwillImpact()
		{
			compClass = typeof(CompTargetEffect_GoodwillImpact);
		}
	}
}
