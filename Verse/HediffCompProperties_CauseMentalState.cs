namespace Verse
{
	public class HediffCompProperties_CauseMentalState : HediffCompProperties
	{
		public MentalStateDef animalMentalState;

		public MentalStateDef animalMentalStateAlias;

		public MentalStateDef humanMentalState;

		public LetterDef letterDef;

		public float mtbDaysToCauseMentalState;

		public HediffCompProperties_CauseMentalState()
		{
			compClass = typeof(HediffComp_CauseMentalState);
		}
	}
}
