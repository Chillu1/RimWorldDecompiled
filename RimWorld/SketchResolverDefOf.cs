namespace RimWorld
{
	[DefOf]
	public static class SketchResolverDefOf
	{
		public static SketchResolverDef Monument;

		public static SketchResolverDef MonumentRuin;

		public static SketchResolverDef Symmetry;

		public static SketchResolverDef AssignRandomStuff;

		public static SketchResolverDef FloorFill;

		public static SketchResolverDef AddColumns;

		public static SketchResolverDef AddCornerThings;

		public static SketchResolverDef AddThingsCentral;

		public static SketchResolverDef AddWallEdgeThings;

		public static SketchResolverDef AddInnerMonuments;

		public static SketchResolverDef DamageBuildings;

		[MayRequireRoyalty]
		public static SketchResolverDef MechCluster;

		[MayRequireRoyalty]
		public static SketchResolverDef MechClusterWalls;

		static SketchResolverDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SketchResolverDefOf));
		}
	}
}
