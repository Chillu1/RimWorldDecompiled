namespace RimWorld
{
	[DefOf]
	public static class RitualVisualEffectDefOf
	{
		[MayRequireIdeology]
		public static RitualVisualEffectDef Basic;

		static RitualVisualEffectDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RitualVisualEffectDefOf));
		}
	}
}
