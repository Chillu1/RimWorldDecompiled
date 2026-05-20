namespace RimWorld;

[DefOf]
public static class RaidStrategyDefOf
{
	public static RaidStrategyDef ImmediateAttack;

	public static RaidStrategyDef ImmediateAttackFriendly;

	[MayRequireAnomaly]
	public static RaidStrategyDef PsychicRitualSiege;

	[MayRequireAnomaly]
	public static RaidStrategyDef ShamblerAssault;

	static RaidStrategyDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(RaidStrategyDefOf));
	}
}
