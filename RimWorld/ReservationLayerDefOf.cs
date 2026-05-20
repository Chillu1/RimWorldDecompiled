using Verse;

namespace RimWorld
{
	[DefOf]
	public static class ReservationLayerDefOf
	{
		public static ReservationLayerDef Floor;

		public static ReservationLayerDef Ceiling;

		public static ReservationLayerDef Empty;

		static ReservationLayerDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ReservationLayerDefOf));
		}
	}
}
