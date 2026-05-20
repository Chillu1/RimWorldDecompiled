using Verse;

namespace RimWorld
{
	[DefOf]
	public static class ApparelLayerDefOf
	{
		public static ApparelLayerDef OnSkin;

		public static ApparelLayerDef Shell;

		public static ApparelLayerDef Middle;

		public static ApparelLayerDef Belt;

		public static ApparelLayerDef Overhead;

		public static ApparelLayerDef EyeCover;

		static ApparelLayerDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ApparelLayerDefOf));
		}
	}
}
