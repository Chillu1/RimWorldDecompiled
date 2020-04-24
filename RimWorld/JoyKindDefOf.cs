namespace RimWorld
{
	[DefOf]
	public static class JoyKindDefOf
	{
		public static JoyKindDef Meditative;

		public static JoyKindDef Social;

		public static JoyKindDef Gluttonous;

		static JoyKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(JoyKindDefOf));
		}
	}
}
