namespace RimWorld
{
	public class WorkGiver_TakeRoamingAnimalsToPen : WorkGiver_TakeToPen
	{
		public WorkGiver_TakeRoamingAnimalsToPen()
		{
			targetRoamingAnimals = true;
			allowUnenclosedPens = true;
		}
	}
}
