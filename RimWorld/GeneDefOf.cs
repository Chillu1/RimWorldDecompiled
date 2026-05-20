using Verse;

namespace RimWorld;

[DefOf]
public static class GeneDefOf
{
	[MayRequireBiotech]
	public static GeneDef Bloodfeeder;

	[MayRequireBiotech]
	public static GeneDef Deathless;

	[MayRequireBiotech]
	public static GeneDef XenogermReimplanter;

	[MayRequireBiotech]
	public static GeneDef Hemogenic;

	[MayRequireBiotech]
	public static GeneDef FireTerror;

	[MayRequireBiotech]
	public static GeneDef Inbred;

	[MayRequireBiotech]
	public static GeneDef WebbedPhalanges;

	static GeneDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(GeneDefOf));
	}
}
