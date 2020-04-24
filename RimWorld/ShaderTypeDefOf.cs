using Verse;

namespace RimWorld
{
	[DefOf]
	public static class ShaderTypeDefOf
	{
		public static ShaderTypeDef Cutout;

		public static ShaderTypeDef CutoutComplex;

		public static ShaderTypeDef Transparent;

		public static ShaderTypeDef MetaOverlay;

		public static ShaderTypeDef EdgeDetect;

		static ShaderTypeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ShaderTypeDef));
		}
	}
}
