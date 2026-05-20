using Verse.Sound;

namespace RimWorld;

[DefOf]
public static class ImpactSoundTypeDefOf
{
	public static ImpactSoundTypeDef Bullet;

	static ImpactSoundTypeDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(ImpactSoundTypeDefOf));
	}
}
