namespace RimWorld
{
	[DefOf]
	public static class RaidStrategyDefOf
	{
		public static RaidStrategyDef ImmediateAttack;

		public static RaidStrategyDef ImmediateAttackFriendly;

		static RaidStrategyDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RaidStrategyDefOf));
		}
	}
}
