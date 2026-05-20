namespace RimWorld;

[DefOf]
public static class LearningDesireDefOf
{
	[MayRequireBiotech]
	public static LearningDesireDef Lessontaking;

	[MayRequireBiotech]
	public static LearningDesireDef Workwatching;

	[MayRequireBiotech]
	public static LearningDesireDef Reading;

	static LearningDesireDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(LearningDesireDefOf));
	}
}
