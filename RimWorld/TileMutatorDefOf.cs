namespace RimWorld;

[DefOf]
public static class TileMutatorDefOf
{
	public static TileMutatorDef Mountain;

	public static TileMutatorDef Coast;

	public static TileMutatorDef River;

	public static TileMutatorDef Caves;

	[MayRequireOdyssey]
	public static TileMutatorDef Lakeshore;

	[MayRequireOdyssey]
	public static TileMutatorDef RiverConfluence;

	[MayRequireOdyssey]
	public static TileMutatorDef Headwater;

	[MayRequireOdyssey]
	public static TileMutatorDef RiverIsland;

	[MayRequireOdyssey]
	public static TileMutatorDef RiverDelta;

	[MayRequireOdyssey]
	public static TileMutatorDef Fjord;

	[MayRequireOdyssey]
	public static TileMutatorDef AnimalHabitat;

	[MayRequireOdyssey]
	public static TileMutatorDef Crevasse;

	[MayRequireOdyssey]
	public static TileMutatorDef MixedBiome;

	static TileMutatorDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(TileMutatorDefOf));
	}
}
