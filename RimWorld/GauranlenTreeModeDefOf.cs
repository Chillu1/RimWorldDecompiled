namespace RimWorld
{
	[DefOf]
	public static class GauranlenTreeModeDefOf
	{
		[MayRequireIdeology]
		public static GauranlenTreeModeDef Gaumaker;

		static GauranlenTreeModeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(GauranlenTreeModeDefOf));
		}
	}
}
