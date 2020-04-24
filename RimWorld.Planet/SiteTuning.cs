using Verse;

namespace RimWorld.Planet
{
	public static class SiteTuning
	{
		public static readonly IntRange QuestSiteTimeoutDaysRange = new IntRange(12, 28);

		public static readonly FloatRange SitePointRandomFactorRange = new FloatRange(0.7f, 1.3f);

		public static readonly SimpleCurve ThreatPointsToSiteThreatPointsCurve = new SimpleCurve
		{
			new CurvePoint(100f, 120f),
			new CurvePoint(1000f, 300f),
			new CurvePoint(2000f, 600f),
			new CurvePoint(3000f, 800f),
			new CurvePoint(4000f, 900f),
			new CurvePoint(5000f, 1000f)
		};
	}
}
