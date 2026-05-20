namespace RimWorld;

[DefOf]
public static class ScenPartDefOf
{
	public static ScenPartDef PlayerFaction;

	public static ScenPartDef ConfigPage_ConfigureStartingPawns;

	public static ScenPartDef PlayerPawnsArriveMethod;

	public static ScenPartDef PlanetLayerFixed;

	static ScenPartDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ScenPartDefOf));
	}
}
