using Verse;

namespace RimWorld;

[DefOf]
public static class ShaderTypeDefOf
{
	public static ShaderTypeDef Cutout;

	public static ShaderTypeDef CutoutHair;

	public static ShaderTypeDef CutoutTiling;

	public static ShaderTypeDef CutoutComplex;

	public static ShaderTypeDef Transparent;

	public static ShaderTypeDef MetaOverlay;

	public static ShaderTypeDef EdgeDetect;

	public static ShaderTypeDef TerrainFadeRoughLinearAdd;

	public static ShaderTypeDef MoteGlow;

	[MayRequireBiotech]
	public static ShaderTypeDef TerrainFadeRoughLinearBurn;

	static ShaderTypeDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ShaderTypeDefOf));
	}
}
