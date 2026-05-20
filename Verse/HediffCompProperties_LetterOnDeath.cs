namespace Verse
{
	public class HediffCompProperties_LetterOnDeath : HediffCompProperties
	{
		public LetterDef letterDef;

		[MustTranslate]
		public string letterText;

		[MustTranslate]
		public string letterLabel;

		public bool onlyIfNoMechanitorDied;

		public HediffCompProperties_LetterOnDeath()
		{
			compClass = typeof(HediffComp_LetterOnDeath);
		}
	}
}
