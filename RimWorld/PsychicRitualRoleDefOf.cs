namespace RimWorld;

[DefOf]
public static class PsychicRitualRoleDefOf
{
	[MayRequireAnomaly]
	public static PsychicRitualRoleDef Invoker;

	[MayRequireAnomaly]
	public static PsychicRitualRoleDef Chanter;

	[MayRequireAnomaly]
	public static PsychicRitualRoleDef ChanterAdvanced;

	[MayRequireAnomaly]
	public static PsychicRitualRoleDef Defender;

	static PsychicRitualRoleDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(PsychicRitualRoleDefOf));
	}
}
