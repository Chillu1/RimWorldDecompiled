namespace RimWorld
{
	public class StorytellerCompProperties_CategoryIndividualMTBByBiome : StorytellerCompProperties
	{
		public IncidentCategoryDef category;

		public bool applyCaravanVisibility;

		public StorytellerCompProperties_CategoryIndividualMTBByBiome()
		{
			compClass = typeof(StorytellerComp_CategoryIndividualMTBByBiome);
		}
	}
}
