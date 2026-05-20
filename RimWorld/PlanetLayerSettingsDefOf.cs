namespace RimWorld;

[DefOf]
public static class PlanetLayerSettingsDefOf
{
	public static PlanetLayerSettingsDef Surface;

	[MayRequireOdyssey]
	public static PlanetLayerSettingsDef Orbit;

	static PlanetLayerSettingsDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(PlanetLayerSettingsDefOf));
	}
}
