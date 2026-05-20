namespace RimWorld;

[DefOf]
public static class BodyTypeDefOf
{
	public static BodyTypeDef Male;

	public static BodyTypeDef Female;

	public static BodyTypeDef Thin;

	public static BodyTypeDef Hulk;

	public static BodyTypeDef Fat;

	[MayRequireBiotech]
	public static BodyTypeDef Baby;

	[MayRequireBiotech]
	public static BodyTypeDef Child;

	static BodyTypeDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(BodyTypeDefOf));
	}
}
