namespace RimWorld
{
	[DefOf]
	public static class ShipJobDefOf
	{
		[MayRequireAnyOf("Ludeon.RimWorld.Royalty,Ludeon.RimWorld.Biotech")]
		public static ShipJobDef Arrive;

		[MayRequireRoyalty]
		public static ShipJobDef FlyAway;

		[MayRequireAnyOf("Ludeon.RimWorld.Royalty,Ludeon.RimWorld.Biotech")]
		public static ShipJobDef WaitTime;

		[MayRequireRoyalty]
		public static ShipJobDef WaitForever;

		[MayRequireAnyOf("Ludeon.RimWorld.Royalty,Ludeon.RimWorld.Biotech")]
		public static ShipJobDef Unload;

		[MayRequireRoyalty]
		public static ShipJobDef Unload_Destination;

		[MayRequireRoyalty]
		public static ShipJobDef WaitSendable;

		static ShipJobDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ShipJobDefOf));
		}
	}
}
