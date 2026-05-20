using System;
using Verse;

namespace RimWorld;

public class CompProperties_Activity : CompProperties
{
	public float changePerDayBase = 0.1f;

	public float? changePerDayOutside;

	public float changePerDayElectroharvester = 0.15f;

	public float changePerDamage = 0.005f;

	public float warning = 0.7f;

	public bool dirtyGraphicsOnActivityChange;

	public FloatRange startingRange = FloatRange.ZeroToOne;

	public Type workerClass = typeof(PawnActivityWorker);

	public SimpleCurve activityResearchFactorCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.5f),
		new CurvePoint(0.5f, 1f),
		new CurvePoint(0.99f, 2f)
	};

	public SimpleCurve damagedActivityMultiplierCurve = new SimpleCurve { new CurvePoint(1f, 1f) };

	public bool showLetterOnActivated;

	public bool showLetterOnManualActivation;

	public bool requiresHoldingPlatform;

	public LetterDef letterDef;

	[MustTranslate]
	public string letterTitle;

	[MustTranslate]
	public string letterDesc;

	private ActivityWorker workerInt;

	public ActivityWorker Worker => workerInt ?? (workerInt = (ActivityWorker)Activator.CreateInstance(workerClass));

	public CompProperties_Activity()
	{
		compClass = typeof(CompActivity);
	}
}
