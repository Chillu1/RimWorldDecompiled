namespace RimWorld;

[DefOf]
public static class FleshTypeDefOf
{
	public static FleshTypeDef Normal;

	public static FleshTypeDef Mechanoid;

	public static FleshTypeDef Insectoid;

	[MayRequireAnomaly]
	public static FleshTypeDef EntityMechanical;

	[MayRequireAnomaly]
	public static FleshTypeDef EntityFlesh;

	[MayRequireAnomaly]
	public static FleshTypeDef Fleshbeast;

	[MayRequireOdyssey]
	public static FleshTypeDef Drone;

	static FleshTypeDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(FleshTypeDefOf));
	}
}
