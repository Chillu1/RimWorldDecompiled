using Verse;

namespace RimWorld;

[DefOf]
public static class MechWeightClassDefOf
{
	public static MechWeightClassDef Light;

	public static MechWeightClassDef Medium;

	public static MechWeightClassDef Heavy;

	public static MechWeightClassDef UltraHeavy;

	static MechWeightClassDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(MechWeightClassDefOf));
	}
}
