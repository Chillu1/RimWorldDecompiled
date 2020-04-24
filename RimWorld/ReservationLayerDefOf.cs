using Verse;

namespace RimWorld
{
	[DefOf]
	public static class ReservationLayerDefOf
	{
		public static ReservationLayerDef Floor;

		public static ReservationLayerDef Ceiling;

		static ReservationLayerDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ReservationLayerDefOf));
		}
	}
}
