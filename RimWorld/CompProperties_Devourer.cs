using Verse;

namespace RimWorld;

public class CompProperties_Devourer : CompProperties
{
	[MustTranslate]
	public string messageDigested;

	[MustTranslate]
	public string messageEmerged;

	[MustTranslate]
	public string messageEmergedCorpse;

	[MustTranslate]
	public string messageDigestionCompleted;

	[MustTranslate]
	public string digestingInspector;

	public int completeDigestionDamage = 125;

	public SimpleCurve bodySizeDigestTimeCurve = new SimpleCurve
	{
		new CurvePoint(0.2f, 10f),
		new CurvePoint(1f, 60f),
		new CurvePoint(3.5f, 90f)
	};

	public SimpleCurve timeDamageCurve = new SimpleCurve
	{
		new CurvePoint(0f, 5f),
		new CurvePoint(60f, 35f)
	};

	public CompProperties_Devourer()
	{
		compClass = typeof(CompDevourer);
	}
}
