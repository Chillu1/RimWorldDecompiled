namespace RimWorld
{
	[DefOf]
	public static class PrisonerInteractionModeDefOf
	{
		public static PrisonerInteractionModeDef NoInteraction;

		public static PrisonerInteractionModeDef AttemptRecruit;

		public static PrisonerInteractionModeDef ReduceResistance;

		public static PrisonerInteractionModeDef Release;

		public static PrisonerInteractionModeDef Execution;

		static PrisonerInteractionModeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PrisonerInteractionModeDefOf));
		}
	}
}
