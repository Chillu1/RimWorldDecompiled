namespace RimWorld;

[DefOf]
public static class PlanetLayerDefOf
{
	public static PlanetLayerDef Surface;

	[MayRequireOdyssey]
	public static PlanetLayerDef Orbit;

	static PlanetLayerDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(PlanetLayerDefOf));
	}
}
