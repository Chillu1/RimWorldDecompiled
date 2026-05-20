namespace Verse
{
	public class StyleCategoryPair : IExposable
	{
		public StyleCategoryDef category;

		public ThingStyleDef styleDef;

		public void ExposeData()
		{
			Scribe_Defs.Look(ref category, "category");
			Scribe_Defs.Look(ref styleDef, "styleDef");
		}
	}
}
