using Verse;

namespace RimWorld;

public class CompProperties_FleshmassNucleus : CompProperties
{
	public float activityOnRoofCollapsed = 0.3f;

	public SimpleCurve activityMeatPerDayCurve = new SimpleCurve
	{
		new CurvePoint(0.1f, 20f),
		new CurvePoint(0.9f, 120f)
	};

	public CompProperties_FleshmassNucleus()
	{
		compClass = typeof(CompFleshmassNucleus);
	}
}
