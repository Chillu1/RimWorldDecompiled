using Verse;

namespace RimWorld
{
	public class CompProperties_ProximityLetter : CompProperties
	{
		public int radius = 4;

		[MustTranslate]
		public string letterLabel;

		[MustTranslate]
		public string letterText;

		public LetterDef letterDef;

		public CompProperties_ProximityLetter()
		{
			compClass = typeof(CompProximityLetter);
		}
	}
}
