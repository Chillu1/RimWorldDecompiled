namespace RimWorld
{
	public class StorytellerCompProperties_NoxiousHaze : StorytellerCompProperties
	{
		public float lastFireMinMTBThreshold = 0.5f;

		public float lastFireMaxMTBThreshold = 2f;

		public StorytellerCompProperties_NoxiousHaze()
		{
			compClass = typeof(StorytellerComp_NoxiousHaze);
		}
	}
}
