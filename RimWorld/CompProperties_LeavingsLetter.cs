using Verse;

namespace RimWorld
{
	public class CompProperties_LeavingsLetter : CompProperties
	{
		[MustTranslate]
		public string letterLabel;

		[MustTranslate]
		public string letterText;

		public LetterDef letterDef;

		public ThingFilter leavingsFilter;

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			leavingsFilter?.ResolveReferences();
		}

		public CompProperties_LeavingsLetter()
		{
			compClass = typeof(CompLeavingsLetter);
		}
	}
}
