using Verse;

namespace RimWorld
{
	public class CompProperties_Art : CompProperties
	{
		public RulePackDef nameMaker;

		public RulePackDef descriptionMaker;

		public QualityCategory minQualityForArtistic;

		public bool mustBeFullGrave;

		public bool canBeEnjoyedAsArt;

		public CompProperties_Art()
		{
			compClass = typeof(CompArt);
		}
	}
}
