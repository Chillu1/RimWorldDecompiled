using Verse;

namespace RimWorld
{
	[DefOf]
	public static class ImplementOwnerTypeDefOf
	{
		public static ImplementOwnerTypeDef Weapon;

		public static ImplementOwnerTypeDef Bodypart;

		public static ImplementOwnerTypeDef Hediff;

		public static ImplementOwnerTypeDef Terrain;

		public static ImplementOwnerTypeDef NativeVerb;

		static ImplementOwnerTypeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ImplementOwnerTypeDefOf));
		}
	}
}
