namespace RimWorld
{
	[DefOf]
	public static class JoyGiverDefOf
	{
		public static JoyGiverDef Play_Chess;

		public static JoyGiverDef Play_Poker;

		static JoyGiverDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(JoyGiverDefOf));
		}
	}
}
