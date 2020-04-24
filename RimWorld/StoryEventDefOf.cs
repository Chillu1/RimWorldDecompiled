namespace RimWorld
{
	[DefOf]
	public static class StoryEventDefOf
	{
		public static StoryEventDef DamageTaken;

		public static StoryEventDef DamageDealt;

		public static StoryEventDef AttackedPlayer;

		public static StoryEventDef KilledPlayer;

		public static StoryEventDef TendedByPlayer;

		public static StoryEventDef Seen;

		public static StoryEventDef TaleCreated;

		static StoryEventDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(StoryEventDefOf));
		}
	}
}
