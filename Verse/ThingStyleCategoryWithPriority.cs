namespace Verse
{
	public class ThingStyleCategoryWithPriority : IExposable
	{
		public StyleCategoryDef category;

		public float priority;

		public ThingStyleCategoryWithPriority()
		{
		}

		public ThingStyleCategoryWithPriority(StyleCategoryDef category, float priority)
		{
			this.category = category;
			this.priority = priority;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref category, "category");
			Scribe_Values.Look(ref priority, "priority", 0f);
		}
	}
}
