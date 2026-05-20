namespace RimWorld
{
	[DefOf]
	public static class SlaveInteractionModeDefOf
	{
		[MayRequireIdeology]
		public static SlaveInteractionModeDef NoInteraction;

		[MayRequireIdeology]
		public static SlaveInteractionModeDef Imprison;

		[MayRequireIdeology]
		public static SlaveInteractionModeDef Suppress;

		[MayRequireIdeology]
		public static SlaveInteractionModeDef Emancipate;

		[MayRequireIdeology]
		public static SlaveInteractionModeDef Execute;

		static SlaveInteractionModeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SlaveInteractionModeDefOf));
		}
	}
}
