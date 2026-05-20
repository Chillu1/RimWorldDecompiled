namespace RimWorld
{
	[DefOf]
	public static class TattooDefOf
	{
		public static TattooDef NoTattoo_Face;

		public static TattooDef NoTattoo_Body;

		static TattooDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TattooDefOf));
		}
	}
}
