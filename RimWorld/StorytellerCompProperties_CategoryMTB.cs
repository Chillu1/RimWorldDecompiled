using Verse;

namespace RimWorld
{
	public class StorytellerCompProperties_CategoryMTB : StorytellerCompProperties
	{
		public float mtbDays = -1f;

		public SimpleCurve mtbDaysFactorByDaysPassedCurve;

		public IncidentCategoryDef category;

		public StorytellerCompProperties_CategoryMTB()
		{
			compClass = typeof(StorytellerComp_CategoryMTB);
		}
	}
}
