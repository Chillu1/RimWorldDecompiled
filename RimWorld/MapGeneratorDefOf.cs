using Verse;

namespace RimWorld;

[DefOf]
public static class MapGeneratorDefOf
{
	public static MapGeneratorDef Encounter;

	public static MapGeneratorDef Base_Player;

	public static MapGeneratorDef Base_Faction;

	[MayRequireAnomaly]
	public static MapGeneratorDef Undercave;

	[MayRequireAnomaly]
	public static MapGeneratorDef MetalHell;

	[MayRequireAnomaly]
	public static MapGeneratorDef Labyrinth;

	[MayRequireOdyssey]
	public static MapGeneratorDef SpacePocket;

	[MayRequireOdyssey]
	public static MapGeneratorDef Space;

	[MayRequireOdyssey]
	public static MapGeneratorDef OrbitalRelay;

	[MayRequireOdyssey]
	public static MapGeneratorDef Mechhive;

	[MayRequireOdyssey]
	public static MapGeneratorDef InsectLair;

	static MapGeneratorDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(MapGeneratorDefOf));
	}
}
