namespace RimWorld
{
	[DefOf]
	public static class PawnsArrivalModeDefOf
	{
		public static PawnsArrivalModeDef EdgeWalkIn;

		public static PawnsArrivalModeDef CenterDrop;

		public static PawnsArrivalModeDef EdgeDrop;

		public static PawnsArrivalModeDef RandomDrop;

		public static PawnsArrivalModeDef Shuttle;

		static PawnsArrivalModeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnsArrivalModeDefOf));
		}
	}
}
