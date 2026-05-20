namespace RimWorld;

[DefOf]
public static class InfectionPathwayDefOf
{
	public static InfectionPathwayDef PrearrivalGeneric;

	static InfectionPathwayDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(InfectionPathwayDefOf));
	}
}
