namespace RimWorld
{
	[DefOf]
	public static class RitualOutcomeEffectDefOf
	{
		[MayRequireRoyalty]
		public static RitualOutcomeEffectDef BestowingCeremony;

		[MayRequireBiotech]
		public static RitualOutcomeEffectDef ChildBirth;

		static RitualOutcomeEffectDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RitualOutcomeEffectDefOf));
		}
	}
}
