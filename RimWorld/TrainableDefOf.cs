namespace RimWorld
{
	[DefOf]
	public static class TrainableDefOf
	{
		public static TrainableDef Tameness;

		public static TrainableDef Obedience;

		public static TrainableDef Release;

		static TrainableDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TrainableDefOf));
		}
	}
}
