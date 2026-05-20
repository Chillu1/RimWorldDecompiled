using Verse;

namespace RimWorld;

public static class GeneTuning
{
	public static readonly SimpleCurve ComplexityToCreationHoursCurve = new SimpleCurve
	{
		new CurvePoint(0f, 3f),
		new CurvePoint(4f, 5f),
		new CurvePoint(8f, 8f),
		new CurvePoint(12f, 12f),
		new CurvePoint(16f, 17f),
		new CurvePoint(20f, 23f)
	};

	public static readonly SimpleCurve MetabolismToFoodConsumptionFactorCurve = new SimpleCurve
	{
		new CurvePoint(-5f, 2.25f),
		new CurvePoint(0f, 1f),
		new CurvePoint(5f, 0.5f)
	};

	public static readonly IntRange BiostatRange = new IntRange(-5, 5);

	public const int BaseMaxComplexity = 6;

	public const float ChemicalDependencyDurationDays_Deficiency = 5f;

	public const float ChemicalDependencyDurationDays_Coma = 30f;

	public const float ChemicalDependencyDurationDays_Death = 60f;

	public const float HemogenGainPerCannibalNutrition = 0.0375f;

	public static readonly FloatRange GeneExtractorRegrowingDurationDaysRange = new FloatRange(12f, 20f);

	public static readonly FloatRange SkinColorValueRange = new FloatRange(0.23f, 0.94f);

	public static readonly FloatRange HairColorValueRange = new FloatRange(0.1f, 0.98f);
}
