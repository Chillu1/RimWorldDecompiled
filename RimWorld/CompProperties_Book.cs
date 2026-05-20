using Verse;

namespace RimWorld;

public class CompProperties_Book : CompProperties_Readable
{
	public RulePackDef nameMaker;

	public RulePackDef descriptionMaker;

	public float pickWeight = 1f;

	public GraphicData openGraphic;

	public GraphicData verticalGraphic;

	public FloatRange ageYearsRange = new FloatRange(5f, 10f);

	public float questChance;

	public CompProperties_Book()
	{
		compClass = typeof(CompBook);
	}
}
