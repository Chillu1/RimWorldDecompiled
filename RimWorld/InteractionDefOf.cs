namespace RimWorld
{
	[DefOf]
	public static class InteractionDefOf
	{
		public static InteractionDef Chitchat;

		public static InteractionDef DeepTalk;

		public static InteractionDef Insult;

		public static InteractionDef RomanceAttempt;

		public static InteractionDef MarriageProposal;

		public static InteractionDef BuildRapport;

		public static InteractionDef RecruitAttempt;

		public static InteractionDef SparkJailbreak;

		public static InteractionDef AnimalChat;

		public static InteractionDef TrainAttempt;

		public static InteractionDef TameAttempt;

		public static InteractionDef Nuzzle;

		static InteractionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(InteractionDefOf));
		}
	}
}
