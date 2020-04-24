using Verse;

namespace RimWorld
{
	[DefOf]
	public static class MapGeneratorDefOf
	{
		public static MapGeneratorDef Encounter;

		public static MapGeneratorDef Base_Player;

		public static MapGeneratorDef Base_Faction;

		public static MapGeneratorDef EscapeShip;

		static MapGeneratorDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MapGeneratorDefOf));
		}
	}
}
