namespace RimWorld;

[DefOf]
public static class LayoutRoomDefOf
{
	[MayRequireAnomaly]
	public static LayoutRoomDef LabyrinthObelisk;

	[MayRequireOdyssey]
	public static LayoutRoomDef AncientRuinsCorridor;

	[MayRequireOdyssey]
	public static LayoutRoomDef AncientRuinsReactorCorridor;

	[MayRequireOdyssey]
	public static LayoutRoomDef AncientOrbitalCorridor;

	[MayRequireOdyssey]
	public static LayoutRoomDef AncientRuinsReactor;

	[MayRequireOdyssey]
	public static LayoutRoomDef AncientRuinsTerraformer;

	[MayRequireOdyssey]
	public static LayoutRoomDef MechanoidEngineRoom;

	[MayRequireOdyssey]
	public static LayoutRoomDef AncientEngineRoom;

	[MayRequireOdyssey]
	public static LayoutRoomDef Mechhive_Stabilizer;

	static LayoutRoomDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(LayoutRoomDefOf));
	}
}
