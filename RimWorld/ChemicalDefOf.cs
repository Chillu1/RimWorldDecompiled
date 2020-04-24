namespace RimWorld
{
	[DefOf]
	public static class ChemicalDefOf
	{
		public static ChemicalDef Alcohol;

		static ChemicalDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ChemicalDefOf));
		}
	}
}
