using Verse;

namespace RimWorld;

[DefOf]
public static class CreepJoinerDownsideDefOf
{
	[MayRequireAnomaly]
	public static CreepJoinerDownsideDef OrganDecay;

	[MayRequireAnomaly]
	public static CreepJoinerDownsideDef CrumblingMind;

	static CreepJoinerDownsideDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(CreepJoinerDownsideDefOf));
	}
}
