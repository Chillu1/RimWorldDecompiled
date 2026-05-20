namespace RimWorld
{
	[DefOf]
	public static class BeardDefOf
	{
		public static BeardDef NoBeard;

		static BeardDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BeardDefOf));
		}
	}
}
