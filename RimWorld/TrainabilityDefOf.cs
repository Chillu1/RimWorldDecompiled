using Verse;

namespace RimWorld
{
	[DefOf]
	public static class TrainabilityDefOf
	{
		public static TrainabilityDef None;

		public static TrainabilityDef Intermediate;

		public static TrainabilityDef Advanced;

		static TrainabilityDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TrainabilityDefOf));
		}
	}
}
