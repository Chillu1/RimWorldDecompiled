namespace RimWorld
{
	[DefOf]
	public static class TransportShipDefOf
	{
		[MayRequireRoyalty]
		public static TransportShipDef Ship_Shuttle;

		[MayRequireBiotech]
		public static TransportShipDef Ship_ShuttleCrashing;

		[MayRequireBiotech]
		public static TransportShipDef Ship_ShuttleCrashing_Mechanitor;

		static TransportShipDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TransportShipDefOf));
		}
	}
}
