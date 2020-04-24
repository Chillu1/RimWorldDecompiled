namespace RimWorld
{
	[DefOf]
	public static class RecordDefOf
	{
		public static RecordDef Kills;

		public static RecordDef KillsHumanlikes;

		public static RecordDef KillsAnimals;

		public static RecordDef KillsMechanoids;

		public static RecordDef PawnsDowned;

		public static RecordDef PawnsDownedHumanlikes;

		public static RecordDef PawnsDownedAnimals;

		public static RecordDef PawnsDownedMechanoids;

		public static RecordDef ShotsFired;

		public static RecordDef Headshots;

		public static RecordDef DamageDealt;

		public static RecordDef DamageTaken;

		public static RecordDef TimesInMentalState;

		public static RecordDef TimesOnFire;

		public static RecordDef FiresExtinguished;

		public static RecordDef OperationsReceived;

		public static RecordDef OperationsPerformed;

		public static RecordDef TimesTendedTo;

		public static RecordDef TimesTendedOther;

		public static RecordDef PeopleCaptured;

		public static RecordDef PrisonersRecruited;

		public static RecordDef PrisonersChatted;

		public static RecordDef AnimalsTamed;

		public static RecordDef AnimalsSlaughtered;

		public static RecordDef MealsCooked;

		public static RecordDef ThingsConstructed;

		public static RecordDef ThingsInstalled;

		public static RecordDef ThingsRepaired;

		public static RecordDef ThingsCrafted;

		public static RecordDef ThingsHauled;

		public static RecordDef PlantsSown;

		public static RecordDef PlantsHarvested;

		public static RecordDef CellsMined;

		public static RecordDef MessesCleaned;

		public static RecordDef ResearchPointsResearched;

		public static RecordDef CorpsesBuried;

		public static RecordDef NutritionEaten;

		public static RecordDef BodiesStripped;

		public static RecordDef ThingsUninstalled;

		public static RecordDef ThingsDeconstructed;

		public static RecordDef ArtifactsActivated;

		public static RecordDef ContainersOpened;

		public static RecordDef SwitchesFlicked;

		public static RecordDef TimeAsColonistOrColonyAnimal;

		public static RecordDef TimeAsPrisoner;

		public static RecordDef TimeAsQuestLodger;

		static RecordDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RecordDefOf));
		}
	}
}
