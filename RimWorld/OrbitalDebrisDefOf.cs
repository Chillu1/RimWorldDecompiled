using Verse;

namespace RimWorld;

[DefOf]
public static class OrbitalDebrisDefOf
{
	[MayRequireOdyssey]
	public static OrbitalDebrisDef Asteroid;

	[MayRequireOdyssey]
	public static OrbitalDebrisDef Manmade;

	[MayRequireOdyssey]
	public static OrbitalDebrisDef Mechanoid;

	static OrbitalDebrisDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(OrbitalDebrisDefOf));
	}
}
